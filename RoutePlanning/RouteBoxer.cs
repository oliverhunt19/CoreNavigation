using GoogleApi.Entities.Common;
using GoogleApi.Entities.Maps.Common;
using GoogleApi.Entities.Maps.Directions.Response;
using RoutePlanning;
using UnitsNet;

public class RouteBoxer
{
    private double R; // earth's mean radius in km
    private List<double> latGrid_;
    private List<double> lngGrid_;
    private List<List<int>> grid_;
    private List<LatLngBounds> boxesX_;
    private List<LatLngBounds> boxesY_;

    public RouteBoxer()
    {
        R = 6371; // earth's mean radius in km
    }

    /// <summary>
    /// This finds the bounded route
    /// </summary>
    /// <param name="route">The route you want to bound</param>
    /// <param name="range">The range of the bounded route</param>
    /// <returns></returns>
    public static BoundedRoute Box(Route route, double range)
    {
        RouteBoxer boxer = new RouteBoxer();
        IReadOnlyList<LatLngBounds> bounds = boxer.Box(route.OverviewPath.Line.Select(x=>LatLng.FromGoogleCoordinates(x)).ToList(), range);
        return new BoundedRoute(route, bounds);
    }

    /**
     * Generates boxes for a given route and distance
     *
     * @param {google.maps.LatLng[] | google.maps.Polyline} path The path along
     *           which to create boxes. The path object can be either an Array of
     *           google.maps.LatLng objects or a Maps API v2 or Maps API v3
     *           google.maps.Polyline object.
     * @param {Number} range The distance in kms around the route that the generated
     *           boxes must cover.
     * @return {google.maps.LatLngBounds[]} An array of boxes that covers the whole
     *           path.
     */
    public IReadOnlyList<LatLngBounds> Box(List<LatLng> path, double range)
    {
        // Two dimensional array representing the cells in the grid overlaid on the path
        this.grid_ = null;
        // Array that holds the latitude coordinate of each vertical grid line
        this.latGrid_ = new List<double>();
        // Array that holds the longitude coordinate of each horizontal grid line
        this.lngGrid_ = new List<double>();
        // Array of bounds that cover the whole route formed by merging cells that
        //  the route intersects first horizontally, and then vertically
        this.boxesX_ = new List<LatLngBounds>();
        // Array of bounds that cover the whole route formed by merging cells that
        //  the route intersects first vertically, and then horizontally
        this.boxesY_ = new List<LatLngBounds>();
        // The array of LatLngs representing the vertices of the path
        List<LatLng> vertices = path;//.Line.Select(x=>LatLng.FromGoogleCoordinates(x)).ToList();
        // Build the grid that is overlaid on the route
        this.BuildGrid(vertices, range);
        // Identify the grid cells that the route intersects
        this.FindIntersectingCells(vertices);
        // Merge adjacent intersected grid cells (and their neighbours) into two sets
        //  of bounds, both of which cover them completely
        this.MergeIntersectingCells();
        // Return the set of merged bounds that has the fewest elements
        return (this.boxesX_.Count <= this.boxesY_.Count ?
                this.boxesX_ :
                this.boxesY_);
    }

    /**
     * Generates boxes for a given route and distance
     *
     * @param {LatLng[]} vertices The vertices of the path over which to lay the grid
     * @param {Number} range The spacing of the grid cells.
     */
    private void BuildGrid(List<LatLng> vertices, double range)
    {
        // Create a LatLngBounds object that contains the whole path
        LatLngBounds routeBounds = new LatLngBounds(vertices.First(),vertices.First());
        foreach (LatLng vertex in vertices)
        {
            routeBounds = routeBounds.Extend(vertex);
        }
        // Find the center of the bounding box of the path
        LatLng routeBoundsCenter = routeBounds.GetCenter();
        // Starting from the center define grid lines outwards vertically until they
        //  extend beyond the edge of the bounding box by more than one cell
        this.latGrid_.Add(routeBoundsCenter.Lat);
        // Add lines from the center out to the north
        this.latGrid_.Add(routeBoundsCenter.RhumbDestinationPoint(0, range).Lat);
        for (int i = 2; this.latGrid_[i - 2] < routeBounds.NorthEast.Lat; i++)
        {
            this.latGrid_.Add(routeBoundsCenter.RhumbDestinationPoint(0, range * i).Lat);
        }
        // Add lines from the center out to the south
        for (int i = 1; this.latGrid_[1] > routeBounds.SouthWest.Lat; i++)
        {
            this.latGrid_.Insert(0, routeBoundsCenter.RhumbDestinationPoint(180, range * i).Lat);
        }
        // Starting from the center define grid lines outwards horizontally until they
        //  extend beyond the edge of the bounding box by more than one cell
        this.lngGrid_.Add(routeBoundsCenter.Lng);
        // Add lines from the center out to the east
        this.lngGrid_.Add(routeBoundsCenter.RhumbDestinationPoint(90, range).Lng);
        for (int i = 2; this.lngGrid_[i - 2] < routeBounds.NorthEast.Lng; i++)
        {
            this.lngGrid_.Add(routeBoundsCenter.RhumbDestinationPoint(90, range * i).Lng);
        }
        // Add lines from the center out to the west
        for (int i = 1; this.lngGrid_[1] > routeBounds.SouthWest.Lng; i++)
        {
            this.lngGrid_.Insert(0, routeBoundsCenter.RhumbDestinationPoint(270, range * i).Lng);
        }
        // Create a two dimensional array representing this grid
        this.grid_ = new List<List<int>>();
        for (int i = 0; i < this.lngGrid_.Count; i++)
        {
            this.grid_.Add(new List<int>());
            for (int j = 0; j < this.latGrid_.Count; j++)
            {
                this.grid_[i].Add(0);
            }
        }
    }

    /**
     * Find all of the cells in the overlaid grid that the path intersects
     *
     * @param {LatLng[]} vertices The vertices of the path
     */
    private void FindIntersectingCells(List<LatLng> vertices)
    {
        // Find the cell where the path begins
        int[] hintXY = this.GetCellCoords(vertices[0]);
        // Mark that cell and it's neighbours for inclusion in the boxes
        this.MarkCell(hintXY);
        // Work through each vertex on the path identifying which grid cell it is in
        for (int i = 1; i < vertices.Count; i++)
        {
            // Use the known cell of the previous vertex to help find the cell of this vertex
            int[] gridXY = this.GetGridCoordsFromHint(vertices[i], vertices[i - 1], hintXY);
            if (gridXY[0] == hintXY[0] && gridXY[1] == hintXY[1])
            {
                // This vertex is in the same cell as the previous vertex
                // The cell will already have been marked for inclusion in the boxes
                continue;
            }
            else if ((Math.Abs(hintXY[0] - gridXY[0]) == 1 && hintXY[1] == gridXY[1]) ||
                (hintXY[0] == gridXY[0] && Math.Abs(hintXY[1] - gridXY[1]) == 1))
            {
                // This vertex is in a cell that shares an edge with the previous cell
                // Mark this cell and it's neighbours for inclusion in the boxes
                this.MarkCell(gridXY);
            }
            else
            {
                // This vertex is in a cell that does not share an edge with the previous
                //  cell. This means that the path passes through other cells between
                //  this vertex and the previous vertex, and we must determine which cells
                //  it passes through
                this.GetGridIntersects(vertices[i - 1], vertices[i], hintXY, gridXY);
            }
            // Use this cell to find and compare with the next one
            hintXY = gridXY;
        }
    }

    /**
     * Find the cell a path vertex is in by brute force iteration over the grid
     *
     * @param {LatLng[]} latlng The latlng of the vertex
     * @return {Number[][]} The cell coordinates of this vertex in the grid
     */
    private int[] GetCellCoords(LatLng latlng)
    {
        int x = 0;
        int y = 0;
        for (x = 0; this.lngGrid_[x] < latlng.Lng; x++) { }
        for (y = 0; this.latGrid_[y] < latlng.Lat; y++) { }
        return new int[] { x - 1, y - 1 };
    }

    /**
     * Find the cell a path vertex is in based on the known location of a nearby
     *  vertex. This saves searching the whole grid when working through vertices
     *  on the polyline that are likely to be in close proximity to each other.
     *
     * @param {LatLng[]} latlng The latlng of the vertex to locate in the grid
     * @param {LatLng[]} hintlatlng The latlng of the vertex with a known location
     * @param {Number[]} hint The cell containing the vertex with a known location
     * @return {Number[]} The cell coordinates of the vertex to locate in the grid
     */
    private int[] GetGridCoordsFromHint(LatLng latlng, LatLng hintlatlng, int[] hint)
    {
        int x, y;
        if (latlng.Lng > hintlatlng.Lng)
        {
            for (x = hint[0]; this.lngGrid_[x + 1] < latlng.Lng; x++) { }
        }
        else
        {
            for (x = hint[0]; this.lngGrid_[x] > latlng.Lng; x--) { }
        }
        if (latlng.Lat > hintlatlng.Lat)
        {
            for (y = hint[1]; this.latGrid_[y + 1] < latlng.Lat; y++) { }
        }
        else
        {
            for (y = hint[1]; this.latGrid_[y] > latlng.Lat; y--) { }
        }
        return new int[] { x, y };
    }

    /**
     * Identify the grid squares that a path segment between two vertices
     * intersects with by:
     * 1. Finding the bearing between the start and end of the segment
     * 2. Using the delta between the lat of the start and the lat of each
     *    latGrid boundary to find the distance to each latGrid boundary
     * 3. Finding the lng of the intersection of the line with each latGrid
     *     boundary using the distance to the intersection and bearing of the line
     * 4. Determining the x-coord on the grid of the point of intersection
     * 5. Filling in all squares between the x-coord of the previous intersection
     *     (or start) and the current one (or end) at the current y coordinate,
     *     which is known for the grid line being intersected
     *
     * @param {LatLng} start The latlng of the vertex at the start of the segment
     * @param {LatLng} end The latlng of the vertex at the end of the segment
     * @param {Number[]} startXY The cell containing the start vertex
     * @param {Number[]} endXY The cell containing the vend vertex
     */
    private void GetGridIntersects(LatLng start, LatLng end, int[] startXY, int[] endXY)
    {
        LatLng edgePoint;
        int[] edgeXY;
        double brng = start.RhumbBearingTo(end);         // Step 1.
        LatLng hint = start;
        int[] hintXY = startXY;
        // Handle a line segment that travels south first
        if (end.Lat > start.Lat)
        {
            // Iterate over the east to west grid lines between the start and end cells
            for (int i = startXY[1] + 1; i <= endXY[1]; i++)
            {
                // Find the latlng of the point where the path segment intersects with
                //  this grid line (Step 2 & 3)
                edgePoint = this.GetGridIntersect(start, brng, this.latGrid_[i]);
                // Find the cell containing this intersect point (Step 4)
                edgeXY = this.GetGridCoordsFromHint(edgePoint, hint, hintXY);
                // Mark every cell the path has crossed between this grid and the start,
                //   or the previous east to west grid line it crossed (Step 5)
                this.FillInGridSquares(hintXY[0], edgeXY[0], i - 1);
                // Use the point where it crossed this grid line as the reference for the
                //  next iteration
                hint = edgePoint;
                hintXY = edgeXY;
            }
            // Mark every cell the path has crossed between the last east to west grid
            //  line it crossed and the end (Step 5)
            this.FillInGridSquares(hintXY[0], endXY[0], endXY[1] - 1);
        }
        else
        {
            // Iterate over the east to west grid lines between the start and end cells
            for (int i = startXY[1]; i > endXY[1]; i--)
            {
                // Find the latlng of the point where the path segment intersects with
                //  this grid line (Step 2 & 3)
                edgePoint = this.GetGridIntersect(start, brng, this.latGrid_[i]);
                // Find the cell containing this intersect point (Step 4)
                edgeXY = this.GetGridCoordsFromHint(edgePoint, hint, hintXY);
                // Mark every cell the path has crossed between this grid and the start,
                //   or the previous east to west grid line it crossed (Step 5)
                this.FillInGridSquares(hintXY[0], edgeXY[0], i);
                // Use the point where it crossed this grid line as the reference for the
                //  next iteration
                hint = edgePoint;
                hintXY = edgeXY;
            }
            // Mark every cell the path has crossed between the last east to west grid
            //  line it crossed and the end (Step 5)
            this.FillInGridSquares(hintXY[0], endXY[0], endXY[1]);
        }
    }

    /**
     * Find the latlng at which a path segment intersects with a given
     *   line of latitude
     *
     * @param {LatLng} start The vertex at the start of the path segment
     * @param {Number} brng The bearing of the line from start to end
     * @param {Number} gridLineLat The latitude of the grid line being intersected
     * @return {LatLng} The latlng of the point where the path segment intersects
     *                    the grid line
     */
    private LatLng GetGridIntersect(LatLng start, double brng, double gridLineLat)
    {
        double d = this.R * ((gridLineLat.ToRad() - start.Lat.ToRad()) / Math.Cos(brng.ToRad()));
        return start.RhumbDestinationPoint(brng, d);
    }

    /**
     * Mark all cells in a given row of the grid that lie between two columns
     *   for inclusion in the boxes
     *
     * @param {Number} startx The first column to include
     * @param {Number} endx The last column to include
     * @param {Number} y The row of the cells to include
     */
    private void FillInGridSquares(int startx, int endx, int y)
    {
        if (startx < endx)
        {
            for (int x = startx; x <= endx; x++)
            {
                this.MarkCell(new int[] { x, y });
            }
        }
        else
        {
            for (int x = startx; x >= endx; x--)
            {
                this.MarkCell(new int[] { x, y });
            }
        }
    }

    /**
     * Mark a cell and the 8 immediate neighbours for inclusion in the boxes
     *
     * @param {Number[]} square The cell to mark
     */
    private void MarkCell(int[] cell)
    {
        int x = cell[0];
        int y = cell[1];
        this.grid_[x - 1][y - 1] = 1;
        this.grid_[x][y - 1] = 1;
        this.grid_[x + 1][y - 1] = 1;
        this.grid_[x - 1][y] = 1;
        this.grid_[x][y] = 1;
        this.grid_[x + 1][y] = 1;
        this.grid_[x - 1][y + 1] = 1;
        this.grid_[x][y + 1] = 1;
        this.grid_[x + 1][y + 1] = 1;
    }

    /**
     * Create two sets of bounding boxes, both of which cover all of the cells that
     *   have been marked for inclusion.
     *
     * The first set is created by combining adjacent cells in the same column into
     *   a set of vertical rectangular boxes, and then combining boxes of the same
     *   height that are adjacent horizontally.
     *
     * The second set is created by combining adjacent cells in the same row into
     *   a set of horizontal rectangular boxes, and then combining boxes of the same
     *   width that are adjacent vertically.
     *
     */
    private void MergeIntersectingCells()
    {
        LatLngBounds? currentBox = null;
        // Traverse the grid a row at a time
        for (int y = 0; y < this.grid_[0].Count; y++)
        {
            for (int x = 0; x < this.grid_.Count; x++)
            {
                if (this.grid_[x][y] == 1)
                {
                    // This cell is marked for inclusion. If the previous cell in this
                    //   row was also marked for inclusion, merge this cell into it's box.
                    // Otherwise start a new box.
                    LatLngBounds box = this.GetCellBounds(new int[] { x, y });
                    if (currentBox != null)
                    {
                        currentBox = currentBox.Value.Extend(box.NorthEast);
                    }
                    else
                    {
                        currentBox = box;
                    }
                }
                else
                {
                    // This cell is not marked for inclusion. If the previous cell was
                    //  marked for inclusion, merge it's box with a box that spans the same
                    //  columns from the row below if possible.
                    this.MergeBoxesY(currentBox);
                    currentBox = null;
                }
            }
            // If the last cell was marked for inclusion, merge it's box with a matching
            //  box from the row below if possible.
            this.MergeBoxesY(currentBox);
            currentBox = null;
        }
        // Traverse the grid a column at a time
        for (int x = 0; x < this.grid_.Count; x++)
        {
            for (int y = 0; y < this.grid_[0].Count; y++)
            {
                if (this.grid_[x][y] == 1)
                {
                    // This cell is marked for inclusion. If the previous cell in this
                    //   column was also marked for inclusion, merge this cell into it's box.
                    // Otherwise start a new box.
                    if (currentBox != null)
                    {
                        LatLngBounds box = this.GetCellBounds(new int[] { x, y });
                        currentBox = currentBox.Value.Extend(box.NorthEast);
                    }
                    else
                    {
                        currentBox = this.GetCellBounds(new int[] { x, y });
                    }
                }
                else
                {
                    // This cell is not marked for inclusion. If the previous cell was
                    //  marked for inclusion, merge it's box with a box that spans the same
                    //  rows from the column to the left if possible.
                    this.MergeBoxesX(currentBox);
                    currentBox = null;
                }
            }
            // If the last cell was marked for inclusion, merge it's box with a matching
            //  box from the column to the left if possible.
            this.MergeBoxesX(currentBox);
            currentBox = null;
        }
    }

    /**
     * Search for an existing box in an adjacent row to the given box that spans the
     * same set of columns and if one is found merge the given box into it. If one
     * is not found, append this box to the list of existing boxes.
     *
     * @param {LatLngBounds}  The box to merge
     */
    private void MergeBoxesX(LatLngBounds? box)
    {
        if (box != null)
        {
            for (int i = 0; i < this.boxesX_.Count; i++)
            {
                if (this.boxesX_[i].NorthEast.Lng == box.Value.SouthWest.Lng &&
                    this.boxesX_[i].SouthWest.Lat == box.Value.SouthWest.Lat &&
                    this.boxesX_[i].NorthEast.Lat == box.Value.NorthEast.Lat)
                {
                    boxesX_[i] = this.boxesX_[i].Extend(box.Value.NorthEast);
                    return;
                }
            }
            this.boxesX_.Add(box.Value);
        }
    }

    /**
     * Search for an existing box in an adjacent column to the given box that spans
     * the same set of rows and if one is found merge the given box into it. If one
     * is not found, append this box to the list of existing boxes.
     *
     * @param {LatLngBounds}  The box to merge
     */
    private void MergeBoxesY(LatLngBounds? box)
    {
        if (box != null)
        {
            for (int i = 0; i < this.boxesY_.Count; i++)
            {
                if (this.boxesY_[i].NorthEast.Lat == box.Value.SouthWest.Lat &&
                    this.boxesY_[i].SouthWest.Lng == box.Value.SouthWest.Lng &&
                    this.boxesY_[i].NorthEast.Lng == box.Value.NorthEast.Lng)
                {
                    boxesY_[i] = this.boxesY_[i].Extend(box.Value.NorthEast);
                    return;
                }
            }
            this.boxesY_.Add(box.Value);
        }
    }

    /**
     * Obtain the LatLng of the origin of a cell on the grid
     *
     * @param {Number[]} cell The cell to lookup.
     * @return {LatLng} The latlng of the origin of the cell.
     */
    private LatLngBounds GetCellBounds(int[] cell)
    {
        return new LatLngBounds(
          new LatLng(this.latGrid_[cell[1]], this.lngGrid_[cell[0]]),
          new LatLng(this.latGrid_[cell[1] + 1], this.lngGrid_[cell[0] + 1]));
    }
}

public struct LatLng
{
    public DD Lat { get; set; }
    public DD Lng { get; set; }

    public LatLng(DD lat, DD lng)
    {
        this.Lat = lat;
        this.Lng = lng;
    }

    public LatLng(double lat, double lng):this(new DD(lat),new DD(lng))
    {

    }

    public LatLng(DMS lat, DMS lng) : this(lat.ToDD(), lng.ToDD())
    {

    }

    //public LatLng(double latdeg, double latmin, double lngdeg, double lngmin) : this(latdeg +latmin/60,lngdeg+lngmin/60)
    //{

    //}

    public LatLng RhumbDestinationPoint(double brng, double dist)
    {
        return LatLng.RhumbDestinationPoint(this, brng, dist);
    }

    public double RhumbBearingTo(LatLng dest)
    {
        return LatLng.RhumbBearingTo(this, dest);
    }

    public static LatLng FromGoogleCoordinates(Coordinate coordinate)
    {
        return new LatLng(coordinate.Latitude, coordinate.Longitude);
    }

    public Coordinate ToGoogleCoordinate()
    {
        return new Coordinate(this.Lat, this.Lng);
    }

    public CoordinateEx ToGoogleCoordinateEx()
    {
        return new CoordinateEx(this.Lat, this.Lng);
    }


    /* Based on the Latitude/longitude spherical geodesy formulae & scripts
           at http://www.movable-type.co.uk/scripts/latlong.html
           (c) Chris Veness 2002-2010
           */
    public static LatLng RhumbDestinationPoint(LatLng start, double brng, double dist)
    {
        double R = 6371; // earth's mean radius in km
        double d = dist / R;  // d = angular distance covered on earth's surface
        double lat1 = start.Lat.ToRad(), lon1 = start.Lng.ToRad();
        brng = brng.ToRad();
        double lat2 = lat1 + d * Math.Cos(brng);
        double dLat = lat2 - lat1;
        double dPhi = Math.Log(Math.Tan(lat2 / 2 + Math.PI / 4) / Math.Tan(lat1 / 2 + Math.PI / 4));
        double q = (Math.Abs(dLat) > 1e-10) ? dLat / dPhi : Math.Cos(lat1);
        double dLon = d * Math.Sin(brng) / q;
        // check for going past the pole
        if (Math.Abs(lat2) > Math.PI / 2)
        {
            lat2 = lat2 > 0 ? Math.PI - lat2 : -(Math.PI - lat2);
        }
        double lon2 = (lon1 + dLon + Math.PI) % (2 * Math.PI) - Math.PI;
        if (double.IsNaN(lat2) || double.IsNaN(lon2))
        {
            return default;
        }
        return new LatLng(lat2.ToDeg(), lon2.ToDeg());
        //return RhumbDestinationPointNew(start, new DD(brng), Length.FromKilometers(dist));
    }


    public static LatLng RhumbDestinationPointNew(LatLng start, DD brng, Length dist)
    {
        double R = 6371; // earth's mean radius in km
        double d = dist.Kilometers / R;
        double lat1 = start.Lat.ToRad();
        double lng1 = start.Lng.ToRad();
        double theta = brng.ToRad();

        double lat2 = Math.Asin(Math.Sin(lat1) * Math.Cos(d) + Math.Cos(lat1) * Math.Sin(d) * Math.Cos(theta));
        double atan2_1 = Math.Sin(theta)*Math.Sin(d)*Math.Cos(lat1);
        double atan2_2 = Math.Cos(d)-Math.Sin(lat1)*Math.Sin(lat2);
        double lng2 = lng1 + Math.Atan2(atan2_1, atan2_2);
        return new LatLng(lat2.ToDeg(), lng2.ToDeg());
    }
    public static double RhumbBearingTo(LatLng start, LatLng dest)
    {
        double dLon = (dest.Lng - start.Lng).ToRad();
        double dPhi = Math.Log(Math.Tan(dest.Lat.ToRad() / 2 + Math.PI / 4) / Math.Tan(start.Lat.ToRad() / 2 + Math.PI / 4));
        if (Math.Abs(dLon) > Math.PI)
        {
            dLon = dLon > 0 ? -(2 * Math.PI - dLon) : (2 * Math.PI + dLon);
        }
        return Math.Atan2(dLon, dPhi).ToBrng();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="start"></param>
    /// <param name="end"></param>
    /// <returns></returns>
    /// <see cref="https://www.omnicalculator.com/other/latitude-longitude-distance#obtaining-the-distance-between-two-points-on-earth-distance-between-coordinates"/>
    public static Length DistanceBetweenTwoCoordinates(LatLng start, LatLng end)
    {
        double R = 6371; // earth's mean radius in km
        double T1 = start.Lat.ToRad();
        double T2 = end.Lat.ToRad();

        double P1 = start.Lng.ToRad();
        double P2 = end.Lng.ToRad();

        double t2 = Math.Pow(Math.Sin((T2-T1) /2),2);

        double t3 = Math.Cos(T1) * Math.Cos(T2) * Math.Pow(Math.Sin((P2-P1) / 2), 2);
        double t4 = 2 * R * Math.Asin(Math.Sqrt( t2 + t3));
        return Length.FromKilometers(t4);
    }

    public override string ToString()
    {
        return $"{Lat}|{Lng}";
    }
}

public static class MathExtensions
{
    public static double ToRad(this double degrees)
    {
        return degrees * Math.PI / 180;
    }

    public static double ToDeg(this double radians)
    {
        return radians * 180 / Math.PI;
    }

    public static double ToBrng(this double degrees)
    {
        return (degrees.ToDeg() + 360) % 360;
    }
}

public struct LatLngBounds : ICloneable
{
    public LatLng NorthEast { get;  }
    public LatLng SouthWest { get; }

    public LatLngBounds(LatLng southWest, LatLng northEast)
    {
        this.SouthWest = southWest;
        this.NorthEast = northEast;
    }

    public LatLngBounds Clone()
    {
        return new LatLngBounds(SouthWest, NorthEast);
    }

    public LatLngBounds Extend(LatLng point)
    {
        double swLat = Math.Min(this.SouthWest.Lat, point.Lat);
        double swLong = Math.Min(this.SouthWest.Lng, point.Lng);
        LatLng newSouthWest = new LatLng(swLat, swLong);


        double neLat = Math.Max(this.NorthEast.Lat, point.Lat);
        double neLon = Math.Max(this.NorthEast.Lng, point.Lng);
        LatLng newNorthEast = new LatLng(neLat, neLon);
        return new LatLngBounds(newSouthWest, newNorthEast);
    }

    public static LatLngBounds GetBounds(IReadOnlyList<LatLng> latLngs)
    {
        if(latLngs.Count < 1)
        {
            throw new ArgumentException("Not enough inputs");
        }
        LatLngBounds latLngBounds = new LatLngBounds(latLngs.First(), latLngs.First());
        foreach(var latlng in latLngs)
        {
            latLngBounds = latLngBounds.Extend(latlng);
        }
        return latLngBounds;
    }

    /// <summary>
    /// Find the centre of the bounding
    /// </summary>
    /// <returns>The latitude and longitude for the centre of the box</returns>
    public LatLng GetCenter()
    {
        double lat = (this.SouthWest.Lat + this.NorthEast.Lat) / 2;
        double lng = (this.SouthWest.Lng + this.NorthEast.Lng) / 2;
        return new LatLng(lat, lng);
    }

    /// <summary>
    /// Finds the distance between the edge of the box and the centre
    /// </summary>
    /// <returns></returns>
    public Length GetDistanceFromCentre()
    {
        LatLng centre = GetCenter();
        return LatLng.DistanceBetweenTwoCoordinates(centre, NorthEast);
    }

    /// <summary>
    /// Finds the bounding box for the given centre and the distance to the corner of the box
    /// </summary>
    /// <param name="centre"></param>
    /// <param name="dist"></param>
    /// <returns></returns>
    public static LatLngBounds GetBoundingBox(LatLng centre, double dist)
    {
        double correctedDistance = dist * Math.Sqrt(2);
        return new LatLngBounds(
            LatLng.RhumbDestinationPoint(centre, 225, correctedDistance), 
            LatLng.RhumbDestinationPoint(centre, 45, correctedDistance));
    }

    

    public IReadOnlyList<T> GetAllInBox<T>(IEnumerable<T> values)
            where T : ILocation
    {
        List<T> Results = new List<T>();
        foreach(T value in values)
        {
            if(ContainedInBox(value))
            {
                Results.Add(value);

            }
        }
        return Results;
    }

    private bool ContainedInLatitude(ILocation location)
    {
        double lat = location.Coordinates.Lat;
        return lat <= NorthEast.Lat && lat >= SouthWest.Lat;
    }
    private bool ContainedInLongitude(ILocation location)
    {
        double lng = location.Coordinates.Lng;
        return lng <= NorthEast.Lng && lng >= SouthWest.Lng;
    }

    /// <summary>
    /// Finds if the location is inside the box
    /// </summary>
    /// <param name="location"></param>
    /// <returns></returns>
    public bool ContainedInBox(ILocation location)
    {
        if (!ContainedInLatitude(location)) return false;
        if(!ContainedInLongitude(location)) return false;
        return true;
    }

    public static LatLngBounds GetLargestBox(IReadOnlyList<LatLngBounds> latLngBounds)
    {
        LatLngBounds firstBox = latLngBounds.First().Clone();
        foreach(LatLngBounds latLng in latLngBounds)
        {
            firstBox =firstBox.Extend(latLng.NorthEast);
            firstBox =firstBox.Extend(latLng.SouthWest);
        }

        return firstBox;
    }

    object ICloneable.Clone()
    {
        return Clone();
    }
}



