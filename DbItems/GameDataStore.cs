using Microsoft.Data.Sqlite;

namespace SampleApp.DbItems
{
    public class GameDataStore : IDisposable
    {

        private readonly string _databaseConnectionString;
        private readonly SqliteConnection _connection;
        private readonly SampleUserStore _userStore;
        private readonly SqliteTransaction? _transaction;

        public GameDataStore(IConfiguration configuration, SampleUserStore userStore)
        {
            if (configuration == null)
                throw new ArgumentNullException(nameof(configuration));
            _databaseConnectionString = configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
            _connection = new SqliteConnection(_databaseConnectionString);
            _connection.Open();
            var options = configuration.GetSection("DatabaseOptions");
            _connection.DefaultTimeout = options.GetValue<int?>("DefaultTimeout") ?? 30;
            InitializeDatabase();
            _userStore = userStore;
            _transaction = _connection.BeginTransaction();

        }

        public void InitializeDatabase()
        {
            using var tableCommand = _connection.CreateCommand();
            tableCommand.CommandText =
            @"
            CREATE TABLE IF NOT EXISTS GameData (
                Id TEXT PRIMARY KEY,
                PlayerXId TEXT,
                PlayerOId TEXT,
                Status INTEGER,
                Board BLOB,
                Name TEXT,
                FOREIGN KEY (PlayerXId) REFERENCES Users(Id),
                FOREIGN KEY (PlayerOId) REFERENCES Users(Id)
            );
            ";
            tableCommand.ExecuteNonQuery();
        }

        public string GetConnectionString()
        {
            return _databaseConnectionString;
        }

        private static byte[] SerializeBoard(GameData.Field[,] board)
        {
            var flatBoard = new byte[board.GetLength(0) * board.GetLength(1)];
            for (int i = 0; i < board.GetLength(0); i++)
            {
                for (int j = 0; j < board.GetLength(1); j++)
                {
                    flatBoard[i * board.GetLength(1) + j] = (byte)board[i, j];
                }
            }
            return flatBoard;
        }

        private static GameData.Field[,] DeserializeBoard(byte[] data, int rows, int cols)
        {
            var board = new GameData.Field[rows, cols];
            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < cols; j++)
                {
                    board[i, j] = (GameData.Field)data[i * cols + j];
                }
            }
            return board;
        }

        public void InsertGameData(GameData gameData)
        {
            using var insertCommand = _connection.CreateCommand();
            insertCommand.CommandText =
            @"
            INSERT INTO GameData (Id, PlayerXId, PlayerOId, Status, Board, Name)
            VALUES ($id, $playerXId, $playerOId, $status, $board, $name);
            ";
            insertCommand.Parameters.AddWithValue("$id", gameData.Id.ToString());
            insertCommand.Parameters.AddWithValue("$playerXId", gameData.PlayerXId?.ToString() ?? (object)DBNull.Value);
            insertCommand.Parameters.AddWithValue("$playerOId", gameData.PlayerOId?.ToString() ?? (object)DBNull.Value);
            insertCommand.Parameters.AddWithValue("$status", (int)gameData.Status);
            insertCommand.Parameters.AddWithValue("$board", SerializeBoard(gameData.Board));
            insertCommand.Parameters.AddWithValue("$name", gameData.Name);
            insertCommand.ExecuteNonQuery();
        }

        public void UpdateGameData(GameData gameData)
        {
            using var updateCommand = _connection.CreateCommand();
            updateCommand.CommandText =
            @"
            UPDATE GameData
            SET PlayerXId = $playerXId,
                PlayerOId = $playerOId,
                Status = $status,
                Board = $board,
                Name = $name
            WHERE Id = $id;
            ";
            updateCommand.Parameters.AddWithValue("$id", gameData.Id.ToString());
            updateCommand.Parameters.AddWithValue("$playerXId", gameData.PlayerXId?.ToString() ?? (object)DBNull.Value);
            updateCommand.Parameters.AddWithValue("$playerOId", gameData.PlayerOId?.ToString() ?? (object)DBNull.Value);
            updateCommand.Parameters.AddWithValue("$status", (int)gameData.Status);
            updateCommand.Parameters.AddWithValue("$board", SerializeBoard(gameData.Board));
            updateCommand.Parameters.AddWithValue("$name", gameData.Name);
            updateCommand.ExecuteNonQuery();
        }

        public GameData GetById(Guid id)
        {
            using var selectCommand = _connection.CreateCommand();
            selectCommand.CommandText =
                @"
                SELECT Id, PlayerXId, PlayerOId, Status, Board, Name
                FROM GameData
                WHERE Id = $id;
                ";
            selectCommand.Parameters.AddWithValue("$id", id.ToString());
            using var reader = selectCommand.ExecuteReader();
            if (reader.Read())
            {
                var gameData = new GameData
                (
                    Guid.Parse(reader.GetString(0)),
                    reader.IsDBNull(1) ? null : reader.GetGuid(1),
                    reader.IsDBNull(2) ? null : reader.GetGuid(2),
                    (GameData.GameStatus)reader.GetInt32(3),
                    DeserializeBoard((byte[])reader["Board"], 3, 3),
                    reader.GetString(5)
                );
                return gameData;
            }
            else
            {
                throw new InvalidOperationException($"GameData with Id {id} not found.");
            }
        }

        public IEnumerable<GameData> GetAllByPlayer(Guid id)
        {
            using var selectCommand = _connection.CreateCommand();
            selectCommand.CommandText =
                @"
                SELECT Id, PlayerXId, PlayerOId, Status, Board, Name
                FROM GameData
                WHERE PlayerXId = $id OR PlayerOId = $id;
                ";
            selectCommand.Parameters.AddWithValue("$id", id.ToString());
            using var reader = selectCommand.ExecuteReader();
            while (reader.Read())
            {
                var gameData = new GameData
                (
                    Guid.Parse(reader.GetString(0)),
                    reader.IsDBNull(1) ? null : reader.GetGuid(1),
                    reader.IsDBNull(2) ? null : reader.GetGuid(2),
                    (GameData.GameStatus)reader.GetInt32(3),
                    DeserializeBoard((byte[])reader["Board"], 3, 3),
                    reader.GetString(5)
                );
                yield return gameData;
            }
        }

        public IEnumerable<GameData> GetAllGames()
        {
            using var selectCommand = _connection.CreateCommand();
            selectCommand.CommandText =
                @"
                SELECT Id, PlayerXId, PlayerOId, Status, Board, Name
                FROM GameData;
                ";
            using var reader = selectCommand.ExecuteReader();
            while (reader.Read())
            {
                var gameData = new GameData
                (
                    Guid.Parse(reader.GetString(0)),
                    reader.IsDBNull(1) ? null : reader.GetGuid(1),
                    reader.IsDBNull(2) ? null : reader.GetGuid(2),
                    (GameData.GameStatus)reader.GetInt32(3),
                    DeserializeBoard((byte[])reader["Board"], 3, 3),
                    reader.GetString(5)
                );
                yield return gameData;
            }
        }

        public void FillUserData(IList<GameData> games)
        {
            IList<Guid> guids = games
                .Where(g => g.PlayerXId.HasValue)
                .Select(g => g.PlayerXId!.Value)
                .Union(games
                .Where(g => g.PlayerOId.HasValue)
                .Select(g => g.PlayerOId!.Value))
                .Distinct()
                .ToList();
            IDictionary<Guid, SampleUser> userCache = _userStore.GetUserListAsync(guids, CancellationToken.None).Result;
            foreach (var game in games)
            {
                if (game.PlayerXId.HasValue && userCache.ContainsKey(game.PlayerXId.Value))
                {
                    var playerX = userCache[game.PlayerXId.Value];
                    game.FillPlayerXData(playerX);
                }
                if (game.PlayerOId.HasValue && userCache.ContainsKey(game.PlayerOId.Value))
                {
                    var playerO = userCache[game.PlayerOId.Value];
                    game.FillPlayerOData(playerO);
                }
            }
        }

        public (int, int, int) GetWinLoseDraw(Guid playerId)
        {
            using var command = _connection.CreateCommand();
            command.CommandText =
                @$"
                SELECT 
                    SUM(CASE 
                        WHEN (Status = {(int)GameData.GameStatus.X_Won} AND PlayerXId = $playerId) OR (Status = {(int)GameData.GameStatus.O_Won} AND PlayerOId = $playerId) THEN 1 
                        ELSE 0 
                    END) AS Wins,
                    SUM(CASE 
                        WHEN (Status = {(int)GameData.GameStatus.X_Won} AND PlayerOId = $playerId) OR (Status = {(int)GameData.GameStatus.O_Won} AND PlayerXId = $playerId) THEN 1 
                        ELSE 0 
                    END) AS Losses,
                    SUM(CASE 
                        WHEN Status = {(int)GameData.GameStatus.Draw} THEN 1 
                        ELSE 0 
                    END) AS Draws
                FROM GameData
                WHERE PlayerXId = $playerId OR PlayerOId = $playerId;
                ";
            command.Parameters.AddWithValue("$playerId", playerId.ToString());
            using var reader = command.ExecuteReader();
            reader.Read();
            return (
                reader.IsDBNull(0) ? 0 : reader.GetInt32(0),
                reader.IsDBNull(0) ? 0 : reader.GetInt32(1),
                reader.IsDBNull(0) ? 0 : reader.GetInt32(2)
            );

        }

        public void Dispose()
        {
            _transaction?.Commit();
            _transaction?.Dispose();
            _connection?.Dispose();
        }
    }
}
