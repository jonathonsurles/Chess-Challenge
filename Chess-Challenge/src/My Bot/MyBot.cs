using ChessChallenge.API;
using static ChessChallenge.Application.ConsoleHelper;

public class MyBot : IChessBot
{

    // Go deeper
    public Move Think(Board board, Timer timer)
    {
        Move bestMove = Move.NullMove;
        int bestEval = -10_000_000;
        int mult = board.IsWhiteToMove ? 1 : -1;
        int depth = 3;
        Move[] moves = board.GetLegalMoves();
        foreach (Move move in moves) {
            board.MakeMove(move);
            int eval = mult * AlphaBetaEvaluation(board, depth, -10_000_000, 10_000_000);
            board.UndoMove(move);
            if (eval > bestEval) {
                bestMove = move;
                bestEval = eval;
            }
        }
        return bestMove;
    }

    private int AlphaBetaEvaluation(Board board, int depth, int alpha, int beta) {
        
        // Base case: perform heuristic evaluation
        if (depth == 0 || board.IsDraw() || board.IsInCheckmate()) {
            return Eval(board);
        }

        int bestEval = board.IsWhiteToMove ? -10_000_000 : 10_000_000;
        
        Move[] moves = board.GetLegalMoves();
        foreach (Move candidate in moves) {

            // Get evaluation of candidate move
            board.MakeMove(candidate);
            int candidateEval = AlphaBetaEvaluation(board, depth - 1, alpha, beta);
            board.UndoMove(candidate);

            // Update best evaluation
            if (board.IsWhiteToMove ^ candidateEval < bestEval) {
                bestEval = candidateEval;
            }

            // Check for pruning
            if ((board.IsWhiteToMove && bestEval > beta) || (!board.IsWhiteToMove && bestEval < alpha)) {
                break;
            }

            // Update pruning variables
            if (board.IsWhiteToMove && bestEval > alpha) {
                alpha = bestEval;
            } else if (!board.IsWhiteToMove && bestEval < beta) {
                beta = bestEval;
            }
        }

        return bestEval;

    }

    // Heuristic evaluation function
    // TODO: positional evaluation
    private int Eval(Board board)
    {  
        // Check for end of game
        int checkmateEval = 1_000_000;
        if (board.IsInCheckmate()) {
            if (!board.IsWhiteToMove) {
                return checkmateEval;
            } else {
                return -1 * checkmateEval;
            }
        } else if (board.IsDraw()) {
            return 0;
        }

        // Get material evaluation
        int[] weights = {100, 300, 300, 500, 900, 0, -100, -300, -300, -500, -900, 0};
        int materialEval = 0;
        PieceList[] lists = board.GetAllPieceLists();
        for (int i = 0; i < lists.Length; i++) {
            materialEval += weights[i] * lists[i].Count;
        }

        return materialEval;
    }

}