namespace SnoopyKnights.Grid
{
    public enum TileType
    {
        Grass,
        Forest, // harvestable trees, blocks movement
        Rock,   // harvestable stone, blocks movement
        Water   // blocks movement and building
    }

    /// <summary>Something occupying tiles (a building). Keeps Grid decoupled from Buildings.</summary>
    public interface ITileOccupant
    {
        bool BlocksMovement { get; }
    }

    public sealed class Tile
    {
        public TileType Type = TileType.Grass;
        public bool HasRoad;
        public int ResourceLeft;          // wood in a Forest tile / stone in a Rock tile
        public ITileOccupant Occupant;    // building standing on this tile, if any

        public bool IsWalkable =>
            (Type == TileType.Grass) &&
            (Occupant == null || !Occupant.BlocksMovement);

        public bool CanBuildOn => Type == TileType.Grass && Occupant == null && !HasRoad;
        public bool CanPlaceRoad => Type == TileType.Grass && Occupant == null && !HasRoad;
    }
}
