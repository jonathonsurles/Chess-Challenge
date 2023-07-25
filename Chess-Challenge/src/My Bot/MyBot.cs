using ChessChallenge.API;
using System.Collections.Generic;
using static ChessChallenge.Application.ConsoleHelper;

public class MyBot : IChessBot
{

    public Move Think(Board board, Timer timer)
    {
        int depth = 4;
        (int eval, Move bestMove) = AlphaBetaEvaluation(board, depth, -10_000_000, 10_000_000, true);
        Log($"eval: {eval}");
        return bestMove;
    }

    private (int eval, Move bestMove) AlphaBetaEvaluation(Board board, int depth, int alpha, int beta, bool canExtend) {
        

        // Base case: perform heuristic evaluation
        if (depth == 0 || board.IsDraw() || board.IsInCheckmate()) {
            return (Eval(board), Move.NullMove);
        }

        int bestEval = board.IsWhiteToMove ? -10_000_000 : 10_000_000;
        Move bestMove = Move.NullMove;
        
        Move[] moves = GetSortedMoves(board);
        
        // TODO: killer move heuristic
        foreach (Move candidateMove in moves) {

            // Get evaluation of candidate move
            board.MakeMove(candidateMove);
            (int candidateEval, _) = AlphaBetaEvaluation(board, depth - 1, alpha, beta, false);
            board.UndoMove(candidateMove);

            // Update best evaluation
            if (board.IsWhiteToMove ^ candidateEval < bestEval) {
                bestEval = candidateEval;
                bestMove = candidateMove;
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

        // For checkmates, we represent o 999999 is +M1, 999998 is +M2, -999999 is -M1, etc.
        if (bestEval > 900_000) {
            bestEval--;
        } else if (bestEval < -900_000) {
            bestEval++;
        }
        return (bestEval, bestMove);

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

        int[] weights = {0, 100, 300, 300, 500, 900, 0};
        int materialEval = 0;
        PieceList[] lists = board.GetAllPieceLists();
        foreach (PieceList list in lists) {
            // Material evaluation
            materialEval += weights[(int)list.TypeOfPieceInList] * list.Count * (list.IsWhitePieceList ? 1 : -1);
        }

        return materialEval;
    }

    // // Yoinked from https://stackoverflow.com/a/12171691/21553030
    // private int countSetBits(ulong value) {
    //     int count = 0;
    //     while (value != 0) {
    //         count++;
    //         value &= value - 1;
    //     }
    //     return count;
    // }

    private Move[] GetSortedMoves(Board board) {
        // Sort by checks, then non-check captures
        // Use insertion sort because we're only ever going to deal
        // with relatively small n, and its code footprint is small
        int i = 0;
        Move[] legalMoves = board.GetLegalMoves();
        Move[] orderedMoves = new Move[legalMoves.Length];
        foreach(Move move in legalMoves) {
            if (MoveIsCheck(board, move)) {
                orderedMoves[i] = move;
                i++;
            }
        }
        foreach(Move move in legalMoves) {
            if (!MoveIsCheck(board, move) && move.IsCapture) {
                orderedMoves[i] = move;
                i++;
            }
        }
        foreach(Move move in legalMoves) {
            if (!MoveIsCheck(board, move) && !move.IsCapture) {
                orderedMoves[i] = move;
                i++;
            }
        }
        return orderedMoves;
    }

    private bool MoveIsCheck(Board board, Move move) {
        board.MakeMove(move);
        bool isCheck = board.IsInCheck();
        board.UndoMove(move);
        return isCheck;
    }
}