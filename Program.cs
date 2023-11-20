var game = new TicTacToeGame();

var player = GetUserPlayer();
bool isUserMove = player is UserPlayer.X or UserPlayer.Both;

PlayGame();
PrintResult(game);

void PlayGame()
{
    // The main loop of the game, where either the user or the computer performs their move.
    while (!game.IsCompleted())
    {
        PrintBoard(game);
        
        var move = isUserMove ? GetUserMove() : game.GetBestMove();
        game.PerformMove(move);

        isUserMove = player is not UserPlayer.None && !isUserMove || player is UserPlayer.Both;
    }
}

void PrintResult(TicTacToeGame game)
{
    PrintBoard(game);

    var winner = game.GetWinner();
    if (winner.HasValue)
    {
        Console.WriteLine($"{winner} won!");
    }
    else
    {
        Console.WriteLine("Draw!");
    }
}


void PrintBoard(TicTacToeGame game)
{
    game.PrintBoard();
    Console.WriteLine();
}

UserPlayer GetUserPlayer()
{
    while (true)
    {
        Console.Write($"Print {UserPlayer.X}, or {UserPlayer.O}, or {UserPlayer.None}, or {UserPlayer.Both}: ");
        var line = Console.ReadLine()?.Trim();
        if (Enum.TryParse(line, out UserPlayer value))
        {
            return value;
        }
    }
}

TicTacToeGame.Cell GetUserMove()
{
    while (true)
    {
        Console.Write("Enter row and column (for example, 0 2): ");
        var line = Console.ReadLine();
        if (TryParse(line, out var row, out var col))
        {
            var move = new TicTacToeGame.Cell(row, col);
            if (game.IsValidMove(move))
            {
                return move;
            }
        }
    }

    bool TryParse(string? line, out int row, out int col)
    {
        row = -1;
        col = -1;
        if (line is null)
        {
            return false;
        }

        var splitted = line.Split(' ', '\t', ',', ';');
        if (splitted.Length != 2)
        {
            return false;
        }

        return int.TryParse(splitted[0].Trim(), out row) && int.TryParse(splitted[1].Trim(), out col);
    }
}

internal enum UserPlayer
{
    O = 0,
    X,
    None,
    Both
}