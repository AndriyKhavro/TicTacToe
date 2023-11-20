public class TicTacToeGame
{
    private const char Empty = ' ';
    private const char X = 'X';
    private const char O = 'O';
    private readonly char[,] _board = {
        { Empty, Empty, Empty },
        { Empty, Empty, Empty },
        { Empty, Empty, Empty }
    };

    private readonly Dictionary<string, int> _minimaxCache = new();
    private readonly IReadOnlyList<Cell> _allCells;
    private readonly IReadOnlyList<Cell[]> _rows;
    private readonly IReadOnlyList<Cell[]> _cols;
    private readonly IReadOnlyList<Cell[]> _diagonals;
    private readonly IReadOnlyList<Cell[]> _possibleSequencesToWin;
    
    private char _currentPlayer = X;

    public TicTacToeGame()
    {
        _allCells = GetAllCells().ToArray();
        _rows = GetRows().ToArray();
        _cols = GetColumns().ToArray();
        _diagonals = GetDiagonals().ToArray();
        _possibleSequencesToWin =_rows.Concat(_cols).Concat(_diagonals).ToArray();
    }

    /// <summary>
    /// Returns True if the game is completed (draw or one of the player won). False otherwise. 
    /// </summary>
    public bool IsCompleted()
    {
        return _allCells.All(cell => _board[cell.Row, cell.Column] != Empty)
               || GetWinner().HasValue;
    }

    /// <summary>
    /// Returns 'X' if X player won.
    /// Returns 'O' if O player won.
    /// Returns null in case of draw or if the game is not completed.
    /// </summary>
    public char? GetWinner()
    {
        var possibleWinners = new char?[] { X, O };

        return possibleWinners.FirstOrDefault(player =>
            _possibleSequencesToWin.Any(seq => seq.All(cell => _board[cell.Row, cell.Column] == player)));
    }

    /// <summary>
    /// Prints the board to console.
    /// </summary>
    public void PrintBoard()
    {
        foreach (var row in GetRows())
        {
            Console.WriteLine(string.Join(" | ", row.Select(cell => _board[cell.Row, cell.Column])));
        }
    }

    /// <summary>
    /// Returns True if the move can be performed.
    /// </summary>
    public bool IsValidMove(Cell move)
    {
        return move.Row >= 0 && move.Row < _board.GetLength(0)
            && move.Column >= 0 && move.Column < _board.GetLength(1)
            && _board[move.Row, move.Column] == Empty;
    }

    /// <summary>
    /// Updates the board with the move.
    /// </summary>
    /// <exception cref="InvalidOperationException">Throws if the move is not valid</exception>
    public void PerformMove(Cell move)
    {
        if (!IsValidMove(move))
        {
            throw new InvalidOperationException("Move is not valid");
        }

        _board[move.Row, move.Column] = _currentPlayer;
        _currentPlayer = _currentPlayer == X ? O : X;
    }

    /// <summary>
    /// Returns the best computer move using Minimax algorithm.
    /// </summary>
    public Cell GetBestMove()
    {
        var availableMoves = GetAvailableMoves().ToArray();

        if (!availableMoves.Any())
        {
            throw new InvalidOperationException("Game is completed");
        }

        if (_currentPlayer == X)
        {
            return availableMoves.MaxBy(move =>
            {
                _board[move.Row, move.Column] = X;
                var result = Minimax(0, O);
                _board[move.Row, move.Column] = Empty;
                return result;
            })!;
        }

        return availableMoves.MinBy(move =>
        {
            _board[move.Row, move.Column] = O;
            var result = Minimax(0, X);
            _board[move.Row, move.Column] = Empty;
            return result;
        })!;
    }

    /// <summary>
    /// Returns all available moves for the current board.
    /// </summary>
    private IEnumerable<Cell> GetAvailableMoves()
    {
        return _allCells.Where(cell => _board[cell.Row, cell.Column] == Empty);
    }

    /// <summary>
    /// Returns the best possible result for the current board (minimum when player is O, and maximum when player is X) using minimax algorithm.
    /// The method essentially searches through the game tree to find the best move for the current player, taking into account the opponent's optimal moves.
    /// It also has the caching mechanism which improves efficiency by avoiding recomputation for previously encountered board states.
    /// </summary>
    /// <param name="depth">Represents how many moves have been already made. Must be between 0 and 9.
    /// Required to make sure the algorithm tries to win with the minimum number of moves</param>
    /// <param name="player">Current player.</param>
    /// <returns></returns>
    private int Minimax(int depth, char player)
    {
        var winner = GetWinner();

        if (winner == X)
        {
            return 10 - depth;
        }

        if (winner == O)
        {
            return -10 + depth;
        }

        if (IsCompleted())
        {
            return 0;
        }

        // Check a cache to see if the result for the current state has been calculated before.
        // If yes, return the cached value, avoiding redundant calculations.
        var cacheKey = GetCacheKey();

        if (_minimaxCache.TryGetValue(cacheKey, out int value))
        {
            return value;
        }

        // Generate a list of available moves and recursively calculate the utility values for each possible move.
        // If the current player is X, it returns the maximum value among the recursive calls.
        // If the current player is O, it returns the minimum value.
        var availableMoves = GetAvailableMoves();

        if (player == X)
        {
            return _minimaxCache[cacheKey] = availableMoves.Max(move =>
            {
                _board[move.Row, move.Column] = X;
                var result = Minimax(depth + 1, O);
                _board[move.Row, move.Column] = Empty;
                return result;
            });
        }

        return _minimaxCache[cacheKey] = availableMoves.Min(move =>
        {
            _board[move.Row, move.Column] = O;
            var result = Minimax(depth + 1, X);
            _board[move.Row, move.Column] = Empty;
            return result;
        });
    }

    private string GetCacheKey() => string.Join(string.Empty, _allCells.Select(cell => _board[cell.Row, cell.Column]));

    private IEnumerable<Cell[]> GetRows()
    {
        return Enumerable.Range(0, _board.GetLength(0)).Select(row =>
            Enumerable.Range(0, _board.GetLength(1)).Select(col => new Cell(row, col)).ToArray());
    }

    private IEnumerable<Cell[]> GetColumns()
    {
        return Enumerable.Range(0, _board.GetLength(1)).Select(col =>
            Enumerable.Range(0, _board.GetLength(0)).Select(row => new Cell(row, col)).ToArray());
    }

    private IEnumerable<Cell[]> GetDiagonals()
    {
        var boardSize = Math.Min(_board.GetLength(0), _board.GetLength(1));
        return new[]
        {
            Enumerable.Range(0, boardSize).Select(i => new Cell(i, i)).ToArray(),
            Enumerable.Range(0, boardSize).Select(i => new Cell(i, boardSize - i - 1)).ToArray()
        };
    }

    private IEnumerable<Cell> GetAllCells()
    {
        return Enumerable.Range(0, _board.GetLength(0)).SelectMany(row =>
            Enumerable.Range(0, _board.GetLength(1)).Select(col => new Cell(row, col)));
    }

    public record Cell(int Row, int Column);
}
