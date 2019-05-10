using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IOANNIS
{
    public class Map
    {
        // Types of map tiles
        public const int EMPTY = 0;
        public const int OBSTACLE = 1;
        public const int ROBOT = 2;
        public const int FINISH = 3;

        // Map contents
        public static int[,] mapTiles;
        public static int[,] copy_mapTiles;

        // Map dimensions
        public static int width, length;

        // Draw map in Console
        public static void Draw()
        {
            Logger.Write("Drawing map...");
            Console.Clear();

            for (int i = 0; i < width - 1; i++)
            {
                for (int j = 0; j < length - 1; j++)
                {
                    switch (mapTiles[i, j])
                    {
                        case EMPTY:
                            Console.Write("[ ]");
                            break;
                        case OBSTACLE:
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.Write($"[X]");
                            Console.ResetColor();
                            break;
                        case ROBOT:
                            Console.ForegroundColor = ConsoleColor.Yellow;
                            Console.Write("[R]");
                            Console.ResetColor();
                            break;
                        case FINISH:
                            Console.ForegroundColor = ConsoleColor.Green;
                            Console.Write("[E]");
                            Console.ResetColor();
                            break;
                    }
                }
                Console.Write("\n");
            }
        }

        public static void Place(Coordinates coor, int whatToPlace)
        {
            int currentTile = mapTiles[coor.x, coor.y];

            if ((currentTile == whatToPlace) || (currentTile == EMPTY) || (currentTile == ROBOT))
            {
                mapTiles[coor.x, coor.y] = whatToPlace;
            }
            else
            {
                Logger.Write($"There was already something else at {coor.x}:{coor.y}");
            }

            if (whatToPlace != OBSTACLE)
            {
                Draw();
            }
            
        }

        public static void ClearTile(Coordinates coor)
        {
            mapTiles[coor.x, coor.y] = EMPTY;
        }

        public static void InitialiseMap(int w, int l)
        {
            // Set values from user preferences
            width = w;
            length = l;

            // Generate all empty tiles from the given size
            mapTiles = new int[length, width];

            // Add random obstacles to map
            CreateAndPlaceObstacles();
        }

        public static int ValueAt(Coordinates coor)
        {
            return mapTiles[coor.x, coor.y];
        }

        public static void Reset()
        {
            mapTiles = copy_mapTiles;
            Draw();
        }

        public static void Backup()
        {
            copy_mapTiles = mapTiles;
        }

        private static void CreateAndPlaceObstacles()
        {
            // initialise number of obsticles 
            int numberOfObsticles = length * width / 4;
            Logger.Write($"Creating {numberOfObsticles} obstacles.");

            // for each obsticle, generate coordinates
            HashSet<Coordinates> obsticleCoordinates = new HashSet<Coordinates>();

            while (numberOfObsticles > 0)
            {
                obsticleCoordinates.Add(Coordinates.generateRandom(width, length));
                numberOfObsticles--;
            }

            // place obstacles
            foreach (Coordinates obstacle in obsticleCoordinates)
            {
                Place(obstacle, OBSTACLE);
            }
        }

    }

    public class Robot
    {
        public static Coordinates coor;

        public static SensorReadings readings;

        public static void Place(int x, int y)
        {
            coor = new Coordinates { x = x, y = y };
            Map.Place(coor, Map.ROBOT);

            UpdateSensorReadings();
        }

        public static string MoveRobotInDirection(string direction)
        {
            string result;

            switch (direction)
            {
                case Direction.FORWARD:
                    result = MoveRobot(new Coordinates { x = coor.x - 1, y = coor.y });
                    break;

                case Direction.LEFT:
                    result = MoveRobot(new Coordinates { x = coor.x, y = coor.y - 1 });
                    break;

                case Direction.RIGHT:
                    result = MoveRobot(new Coordinates { x = coor.x, y = coor.y + 1 });
                    break;

                case Direction.BACKWARD:
                    result = MoveRobot(new Coordinates { x = coor.x + 1, y = coor.y });
                    break;

                default:
                    result = Result.FAILED;
                    break;
            }

            // Update sensor readings after every move
            UpdateSensorReadings();

            return result;
        }

        private static string MoveRobot(Coordinates coorToMoveAt)
        {
            int tileAtNewPosition = Map.ValueAt(coorToMoveAt);

            if ((readings.endOfBack) || (readings.endOfFornt) || (readings.endOfLeft) || (readings.endOfRight))
            {
                return Result.FAILED;
            }

            if (tileAtNewPosition == Map.OBSTACLE)
            {
                return Result.FAILED;
            }
            else
            {
                // Remove robot from previous position
                Map.Place(coor, Map.EMPTY);

                // Place robot at new postion
                Place(coorToMoveAt.x, coorToMoveAt.y);

                return Result.SUCCESS;
            }
        }

        public static void UpdateSensorReadings()
        {
            readings = new SensorReadings(coor, Map.mapTiles);
        }
    }

    public class Coordinates
    {
        public int x;
        public int y;

        public static Coordinates generateRandom(int width, int length)
        {
            return new Coordinates { x = Program.randomNumber.Next(1, width), y = Program.randomNumber.Next(1, length) };
        }
    }

    public class Direction
    {
        public const string FORWARD = "forward";
        public const string LEFT = "left";
        public const string RIGHT = "right";
        public const string BACKWARD = "backward";

        public static string DecodeDirection(double directionValue)
        {
            if (directionValue < 2.5)
            {
                return LEFT;
            }
            else if (directionValue < 5)
            {
                return FORWARD;
            }
            else if (directionValue < 7.5)
            {
                return RIGHT;
            }
            else
            {
                return BACKWARD;
            }
        }
    }

    public class Result
    {
        public const string SUCCESS = "success";
        public const string FAILED = "failed";
    }
}
