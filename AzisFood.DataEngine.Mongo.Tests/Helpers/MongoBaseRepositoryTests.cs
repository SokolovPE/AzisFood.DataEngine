using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using AzisFood.DataEngine.Abstractions.Interfaces;
using AzisFood.DataEngine.Core.Implementations;
using AzisFood.DataEngine.Mongo.Implementations;
using AzisFood.DataEngine.Mongo.Tests.DataAttributes;
using AzisFood.DataEngine.Mongo.Tests.Models;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using Moq;
using Xunit;
using Xunit.Abstractions;

namespace AzisFood.DataEngine.Mongo.Tests.Helpers
{
    public class MongoBaseRepositoryTests : IClassFixture<FakeEntityFixture>
    {
        private readonly ITestOutputHelper _testOutputHelper;
        private readonly FakeEntityFixture _fixture;
        private readonly BaseRepository<FakeEntity> _repository;
        private readonly Mock<ILogger<BaseRepository<FakeEntity>>> _logger;
        private readonly Mock<IMongoDatabase> _mongoDatabase;
        private readonly Mock<IMongoClient> _mongoClient;
        private readonly Mock<IMongoCollection<FakeEntity>> _mongoCollection;
        private readonly Mock<IAsyncCursor<FakeEntity>> _asyncCursor;

        public MongoBaseRepositoryTests(FakeEntityFixture fixture, ITestOutputHelper testOutputHelper)
        {
            _fixture = fixture;
            _testOutputHelper = testOutputHelper;
            
            
            _testOutputHelper.WriteLine("Constructing...");
            var mongoOptions = Mock.Of<IMongoOptions>();
            mongoOptions.DatabaseName = "fake-database";
            mongoOptions.ConnectionString = "fake-connection-string";
            
            _logger = new Mock<ILogger<BaseRepository<FakeEntity>>>();
            _mongoDatabase = new Mock<IMongoDatabase>();
            _mongoClient = new Mock<IMongoClient>();
            _asyncCursor = new Mock<IAsyncCursor<FakeEntity>>();
            _mongoCollection = new Mock<IMongoCollection<FakeEntity>>();
            IDataAccess dataAccess = new MongoDataAccess(_mongoDatabase.Object);
            Setup();
            _repository = new BaseRepository<FakeEntity>(_logger.Object, dataAccess);
        }

        private void Setup()
        {
            
            // Mock MongoCollection.FindAsync
            _mongoCollection.Setup(mc => mc.FindAsync(It.IsAny<FilterDefinition<FakeEntity>>(),
                    It.IsAny<FindOptions<FakeEntity>>(), It.IsAny<CancellationToken>()))
                .Callback<FilterDefinition<FakeEntity>, FindOptions<FakeEntity, FakeEntity>, CancellationToken>(
                    (definition, _, _) =>
                    {
                        if (definition is not ExpressionFilterDefinition<FakeEntity> expr)
                        {
                            _asyncCursor.Setup(cursor => cursor.Current).Returns(_fixture.Items);
                            return;
                        }
                        // Setup cursor to new values
                        _asyncCursor.Setup(cursor => cursor.Current).Returns(_fixture.Items.Where(expr.Expression.Compile()).ToList());
                    })
                .ReturnsAsync(_asyncCursor.Object);
            
            // Mock MongoCollection.InsertOneAsync
            _mongoCollection.Setup(mc =>
                    mc.InsertOneAsync(It.IsAny<FakeEntity>(), It.IsAny<InsertOneOptions>(),
                        It.IsAny<CancellationToken>()))
                .Callback<FakeEntity, InsertOneOptions, CancellationToken>((entity, _, _) =>
                {
                    if (entity == null)
                    {
                        return;
                    }

                    // Setup cursor to new values
                    _asyncCursor.Setup(cursor => cursor.Current).Returns(() =>
                    {
                        // Copy fixture list and return it to affect only one case
                        var tempList = _fixture.Items.ToArray().ToList();
                        tempList.Add(entity);
                        return tempList;
                    });
                });
            
            // Mock async cursor
            _asyncCursor.SetupSequence(cursor => cursor.MoveNext(default))
                .Returns(true).Returns(false);
            _asyncCursor.SetupSequence(cursor => cursor.MoveNextAsync(default))
                .ReturnsAsync(true).ReturnsAsync(false);
            
            // Mock MongoDatabase.GetCollection
            _mongoDatabase.Setup(db =>
                    db.GetCollection<FakeEntity>(It.IsAny<string>(), It.IsAny<MongoCollectionSettings>()))
                .Returns(_mongoCollection.Object);
            
            // Mock MongoClient.GetDatabase
            _mongoClient.Setup(client => client.GetDatabase(It.IsAny<string>(), null))
                .Returns(_mongoDatabase.Object);
        }
        
        [Fact]
        public async Task GetAsync_ReturnsAllEntities()
        {
            var result = (await _repository.GetAsync()).ToList();
            Assert.Equal(FakeEntityFactory.CreatePremade().ToArray(), result.ToArray());
        }

        [Theory]
        [GetByIdData]
        public async Task GetAsync_GivenId_ReturnsExistenceOfEntity(Guid id, bool expected)
        {
            var result = await _repository.GetAsync(id);
            var resultExists = result != default;
            Assert.Equal(expected, resultExists);
        }

        [Fact]
        [Trait("Category", "MongoBaseRepository.Cancellation")]
        public async Task GetAsync_GivenCancellation_ThrowsException()
        {
            var cts = new CancellationTokenSource(0);
            _testOutputHelper.WriteLine($"Got a cancellation: {cts.IsCancellationRequested}");
            var exceptionInfo = await Assert.ThrowsAsync<OperationCanceledException>(async () =>
                await _repository.GetAsync(Guid.Empty, cts.Token));
            _testOutputHelper.WriteLine($"Exception message: {exceptionInfo.Message}");
        }

        [Theory]
        [GetByExpressionData]
        public async Task GetAsync_GivenExpression_ReturnsFilteredEntities(
            Expression<Func<FakeEntity, bool>> expression, int expected)
        {
            var result = (await _repository.GetAsync(expression)).Count();
            _testOutputHelper.WriteLine($"Got {result} entities...");
            Assert.Equal(expected, result);
        }
        
        [Fact]
        [Trait("Category", "MongoBaseRepository.Cancellation")]
        public async Task GetAsync_GivenFilterAndCancellation_ThrowsException()
        {
            var cts = new CancellationTokenSource(0);
            _testOutputHelper.WriteLine($"Got a cancellation: {cts.IsCancellationRequested}");
            var exceptionInfo = await Assert.ThrowsAsync<OperationCanceledException>(async () =>
                await _repository.GetAsync(x=>x.Id.ToString().StartsWith("61"), cts.Token));
            _testOutputHelper.WriteLine($"Exception message: {exceptionInfo.Message}");
        }

        [Fact]
        public async Task CreateAsync_GivenEntity_ReturnsItemsCount()
        {
            var newEntity = new FakeEntity {LongField = 123456, StringField = "text_value"};
            var result = await _repository.CreateAsync(newEntity);
            var newLength = _asyncCursor.Object.Current.Count();
            _testOutputHelper.WriteLine($"Items.Cnt={newLength} inserted: {result}");
            Assert.Equal(11, newLength);
        }
        
        [Fact]
        [Trait("Category", "MongoBaseRepository.Cancellation")]
        public async Task CreateAsync_GivenEntityAndCancellation_ThrowsException()
        {
            var cts = new CancellationTokenSource(0);
            _testOutputHelper.WriteLine($"Got a cancellation: {cts.IsCancellationRequested}");
            var newEntity = new FakeEntity {LongField = 123456, StringField = "text_value"};;
            var exceptionInfo = await Assert.ThrowsAsync<OperationCanceledException>(async () =>
                await _repository.CreateAsync(newEntity, cts.Token));
            _testOutputHelper.WriteLine($"Exception message: {exceptionInfo.Message}");
        }
    }
}