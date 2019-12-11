using System;

namespace HIVE
{
    public enum PlayerColor
    {
        None,
        White,
        Black
    }

    public class Player : IEquatable<Player>
    {
        private PlayerColor color;

        public Player(PlayerColor color)
        {
            this.color = color;
        }

        public PlayerColor Color { get => color; }

        public bool Equals(Player other) => color == other.color;
    }
}
