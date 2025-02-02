namespace GameOfLife.API.Models
{
    public class GameOfLifeBoard
    {
        public Guid Id { get; set; }
        public bool[][] Board { get; set; }

        public GameOfLifeBoard(bool[][] board)
        {
            Id = Guid.NewGuid();
            Board = board;
        }
    }
}
