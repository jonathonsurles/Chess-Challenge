using ChessChallenge.API;
using static ChessChallenge.Application.ConsoleHelper;

public class MyBot : IChessBot
{

    public Move Think(Board board, Timer timer)
    {
        Move[] moves = board.GetLegalMoves();
        Move bestMove = Move.NullMove;
        int bestEval = -100000;
        int mult = board.IsWhiteToMove ? 1 : -1;
        foreach (Move move in moves) {
            int eval = mult * Eval(board, move);
            if (eval > bestEval) {
                bestMove = move;
                bestEval = eval;
            }
        }
        return bestMove;
    }

    private int Eval(Board board, Move move)
    {  
        int[] weights = {100, 300, 300, 500, 900, 30000, -100, -300, -300, -500, -900, -30000};
        int eval = 0;
        board.MakeMove(move);
        PieceList[] lists = board.GetAllPieceLists();
        for (int i = 0; i < lists.Length; i++) {
            eval += weights[i] * lists[i].Count;
        }
        board.UndoMove(move);
        return eval;
    }

}