namespace GameOfLife.API.Models
{
    public class GameOfLifeBoard
    {
        public Guid Id { get; set; }
        public int[][] Board { get; set; }

        public GameOfLifeBoard(int[][] board)
        {
            Id = Guid.NewGuid();
            Board = board;
        }
    }
}
