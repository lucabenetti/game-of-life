namespace GameOfLife.API.Models
{
    public class GameOfLifeBoard
    {
        public Guid Id { get; set; }
        public HashSet<(int, int)> Board { get; set; }

        public GameOfLifeBoard(HashSet<(int, int)> board)
        {
            Id = Guid.NewGuid();
            Board = board;
        }
    }
}
