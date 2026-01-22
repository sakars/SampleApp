using System.Diagnostics.CodeAnalysis;

namespace SampleApp.DbItems
{
    public class GameData
    {


        public enum Field
        {
            Empty = 0,
            X = 1,
            O = 2
        }

        public enum GameStatus
        {
            WaitingForPlayers = 0,
            Draw = 1,
            X_Won = 2,
            O_Won = 3,
            X_Turn = 4,
            O_Turn = 5

        }


        public Guid Id { get; private set; }
        public Field[,] Board { get; private set; } = new Field[3, 3];

        public Guid? PlayerXId { get; private set; } = null;
        public Guid? PlayerOId { get; private set; } = null;

        public SampleUser? PlayerXData { get; private set; } = null;
        public SampleUser? PlayerOData { get; private set; } = null;

        public GameStatus Status { get; private set; } = GameStatus.WaitingForPlayers;

        public string StatusString => Status.ToString();

        public string Name { get; private set; }

        public GameData(string name)
        {
            Id = Guid.NewGuid();
            Board = new Field[3, 3];
            Name = name;
            Status = GameStatus.WaitingForPlayers;

        }

        public GameData(Guid? id, Guid? playerXId, Guid? playerOId, GameStatus status, Field[,] board, string name)
        {
            if (board.Length != 9 || board.Rank != 2 || board.GetLength(0) != 3 || board.GetLength(1) != 3)
                throw new ArgumentException("Board must be a 3x3 array.");

            Id = id ?? Guid.NewGuid();
            PlayerXId = playerXId;
            PlayerOId = playerOId;
            Status = status;
            Board = board;
            Name = name;
        }

        public void AddPlayer(Guid playerGuid)
        {
            if (Random.Shared.Next(2) == 0)
            {
                // If Coin flip succeeds, try to add as O first
                if (PlayerOId == null)
                {
                    PlayerOId = playerGuid;
                    if (PlayerXId != null)
                        Status = GameStatus.X_Turn;
                    return;
                }
            }
            if (PlayerXId == null)
            {
                PlayerXId = playerGuid;
                if (PlayerOId != null)
                    Status = GameStatus.X_Turn;
            }
            else if (PlayerOId == null)
            {
                PlayerOId = playerGuid;
                if (PlayerXId != null)
                    Status = GameStatus.X_Turn;
            }
            else
            {
                throw new InvalidOperationException("Both player slots are already filled.");
            }
        }
        public void FillPlayerXData(SampleUser user)
        {
            if (PlayerXId != user.Id)
                throw new InvalidOperationException("User ID does not match Player X ID.");
            PlayerXData = user;
        }

        public void FillPlayerOData(SampleUser user)
        {
            if (PlayerOId != user.Id)
                throw new InvalidOperationException("User ID does not match Player O ID.");
            PlayerOData = user;
        }

        public void MoveFinished()
        {
            // Check for win conditions
            for (int i = 0; i < 3; i++)
            {
                // Check rows
                if (Board[i, 0] != Field.Empty && Board[i, 0] == Board[i, 1] && Board[i, 1] == Board[i, 2])
                {
                    Status = Board[i, 0] == Field.X ? GameStatus.X_Won : GameStatus.O_Won;
                    return;
                }
                // Check columns
                if (Board[0, i] != Field.Empty && Board[0, i] == Board[1, i] && Board[1, i] == Board[2, i])
                {
                    Status = Board[0, i] == Field.X ? GameStatus.X_Won : GameStatus.O_Won;
                    return;
                }
            }
            // Check diagonals
            if (Board[0, 0] != Field.Empty && Board[0, 0] == Board[1, 1] && Board[1, 1] == Board[2, 2])
            {
                Status = Board[0, 0] == Field.X ? GameStatus.X_Won : GameStatus.O_Won;
                return;
            }
            if (Board[0, 2] != Field.Empty && Board[0, 2] == Board[1, 1] && Board[1, 1] == Board[2, 0])
            {
                Status = Board[0, 2] == Field.X ? GameStatus.X_Won : GameStatus.O_Won;
                return;
            }
            // Check for draw
            if (Board.Cast<Field>().All(f => f != Field.Empty))
            {
                Status = GameStatus.Draw;
                return;
            }
            // Switch turns
            if (Status == GameStatus.X_Turn)
                Status = GameStatus.O_Turn;
            else if (Status == GameStatus.O_Turn)
                Status = GameStatus.X_Turn;

        }

        public bool CanPlaceX(int x, int y)
        {
            return Status == GameStatus.X_Turn && Board[x, y] == Field.Empty;
        }

        public bool CanPlaceO(int x, int y)
        {
            return Status == GameStatus.O_Turn && Board[x, y] == Field.Empty;
        }

        public bool TryPlaceX(int x, int y, [NotNullWhen(false)] out string? failreason)
        {
            failreason = null;
            if (Status != GameStatus.X_Turn)
            {
                failreason = "It's not X's turn.";
                return false;
            }
            if (Board[x, y] != Field.Empty)
            {
                failreason = "Field is already occupied.";
                return false;
            }
            Board[x, y] = Field.X;
            MoveFinished();
            return true;
        }

        public void PlaceX(int x, int y)
        {
            if (!TryPlaceX(x, y, out var reason))
                throw new InvalidOperationException(reason);
        }

        public bool TryPlaceO(int x, int y, [NotNullWhen(false)] out string? failreason)
        {
            failreason = null;
            if (Status != GameStatus.O_Turn)
            {
                failreason = "It's not O's turn.";
                return false;
            }
            if (Board[x, y] != Field.Empty)
            {
                failreason = "Field is already occupied.";
                return false;
            }
            Board[x, y] = Field.O;
            MoveFinished();
            return true;
        }

        public void PlaceO(int x, int y)
        {
            if (!TryPlaceO(x, y, out var reason))
                throw new InvalidOperationException(reason);

        }

    }
}
