using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace IOANNIS
{
    /*
     
       Tom and Danni's attempt at a wandering algorithm (if not navigation)

     */

    class Program
    {
        static void Run()
        {
            try
            {
                // Create map
                Map.InitialiseMap(30, 30);

                // Draw map before robot
                Map.Draw();

                // Place robot on map and get readings
                Robot.Place(10, 10);

                // Place end point
                Map.Place(new Coordinates { x = 25, y = 25 }, Map.FINISH);

                // Make a copy of existing map
                Map.Backup();

                // Generate first population
                List<double[]> weightPopulation = GeneticAlgorithm.GenerateWeightPopulation();

                // placeholders
                List<Chromosome> population = new List<Chromosome>();
                List<double[]> nextGeneration = new List<double[]>();

                while (true)
                {
                    population = TestPopulation(weightPopulation);
                    population = GeneticAlgorithm.RankPopulation(population);
                    population = GeneticAlgorithm.CreateNewGeneration(population);
                    GeneticAlgorithm.Mutate(population);

                    weightPopulation.Clear();
                    foreach (Chromosome chr in population) { weightPopulation.Add(chr.weights); }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Unexpected error... must be your computer or something... Don't use Macs. Try running again...");
                Console.ReadKey();
            }

        }

        static List<Chromosome> TestPopulation(List<double[]> population)
        {
            List<Chromosome> chr = new List<Chromosome>();
            
            foreach (double[] genome in population)
            {
                int numberOfSuccessfulSteps = 0;

                while ((Robot.coor.x != 25) && (Robot.coor.y != 25))
                {
                    double instruction = NeuralNetwork.GenerateInstruction(genome, Robot.readings);

                    string decodedInstruction = Direction.DecodeDirection(instruction);

                    string result = Robot.MoveRobotInDirection(decodedInstruction);

                    if (result == Result.SUCCESS)
                    {
                        numberOfSuccessfulSteps++;
                    }
                    else
                    {
                        Map.Reset();
                        Robot.Place(5,5);
                        break;
                    }

                    if (numberOfSuccessfulSteps > 20)
                    {
                        // To stop it from going in endless loops
                        break;
                    }
                }

                chr.Add(new Chromosome { weights = genome, fitness = numberOfSuccessfulSteps});
            }

            return chr;
        }

        static void Main(string[] args)
        {
            Run();           
        }

        // 0 - moved successfully
        // 1 - hit obstacle
        // 2 - reached end point
        static int MoveRobot(Coordinates oldPosition, Coordinates newPosition)
        {
            int result;
            int valueAtCoordinates = map[newPosition.x, newPosition.y];

            switch (valueAtCoordinates)
            {
                
                case 0: // '0' is for empty tile, return 1

                    // Update map
                    map[oldPosition.x, oldPosition.y] = 0;
                    map[newPosition.x, newPosition.y] = 2;

                    Console.Clear();
                    DrawMap();

                    robotsCoordinates = newPosition;
                    result = 1;
                    break;
                
                case 1: // '1' means obstacle, return 0

                    // Obstacle hit
                    result = 0;
                    break;

                case 3:

                    robotsCoordinates = newPosition;
                    // Reached end point
                    result = 2;
                    break;

                default:
                    return 0;
            }

            sensorReadings = new SensorReadings(robotsCoordinates, map);
            return result;
        }

        static int MoveRobotInDirection(Coordinates coor, string direction)
        {
            int result = 0;

            switch (direction)
            {
                case "forward":
                    result = MoveRobot(coor, new Coordinates { x = coor.x - 1, y = coor.y});
                    break;

                case "left": 
                    result = MoveRobot(coor, new Coordinates { x = coor.x, y = coor.y - 1});
                    break;

                case "right":
                    result = MoveRobot(coor, new Coordinates { x = coor.x, y = coor.y + 1 });
                    break;

                case "backward":
                    result = MoveRobot(coor, new Coordinates { x = coor.x + 1, y = coor.y });
                    break;
            }

            return result;
        }

        static void InitialiseObstacles()
        {
            // initialise number of obsticles 
            int numberOfObsticles = mapLength * mapWidth / 3;

            // for each obsticle, generate coordinates
            HashSet<Coordinates> obsticleCoordinates = new HashSet<Coordinates>();

            while (numberOfObsticles > 0)
            {
                obsticleCoordinates.Add(generateNewCoordinates());
                numberOfObsticles--;
            }

            // place obstacles
            foreach (Coordinates obstacle in obsticleCoordinates)
            {
                map[obstacle.x, obstacle.y] = 1;
            }
        }

        static void DrawMap()
        {
            for (int i = 0; i < mapWidth - 1; i++)
            {
                for (int j = 0; j < mapLength - 1; j++)
                {
                    switch (map[i, j])
                    {
                        case 0:
                            Console.Write("[ ]");
                            break;
                        case 1:
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.Write($"[X]");
                            Console.ResetColor();
                            break;
                        case 2:
                            Console.ForegroundColor = ConsoleColor.Yellow;
                            Console.Write("[R]");
                            Console.ResetColor();
                            break;
                        case 3:
                            Console.ForegroundColor = ConsoleColor.Green;
                            Console.Write("[E]");
                            Console.ResetColor();
                            break;
                    }
                }
                Console.Write("\n");
            }
        }

        static Coordinates generateNewCoordinates()
        {
            return new Coordinates { x = randomNumber.Next(1, mapWidth), y = randomNumber.Next(1, mapLength)};
        }

        public static Random randomNumber = new Random();

        // position of the robot on the map
        public static int[] positionXY = new int[] { 0, 0 };

        public static Coordinates robotsCoordinates;
        public static SensorReadings sensorReadings;

        // map
        public static int[,] map;

        // map constraints - 200 x 200
        public const int mapWidth = 30;
        public const int mapLength = 30;
    }

    public class SensorReadings
    {
        public int left, right, front, back;

        private Coordinates robotPosition;
        private int[,] _map;

        public bool endOfLeft, endOfRight, endOfBack, endOfFornt = false;

        private int maxX = Program.mapWidth - 1;
        private int maxY = Program.mapLength - 1;

        public SensorReadings(Coordinates coor, int[,] map)
        {
            robotPosition = coor;
            _map = map;

            getLeft();
            getRight();
            getFront();
            getBack();
        }

        private void getLeft()
        {
            if (robotPosition.y == 0)
            {
                endOfLeft = true;
            }
            else
            {
                left = 0;

                for (int startIndex = robotPosition.y - 1; startIndex > 0; startIndex--)
                {
                    if (_map[robotPosition.x, startIndex] != 1)
                    {
                        left++;
                    }
                    else
                    {
                        break;
                    }
                }
            }
        }

        private void getRight()
        {
            if (robotPosition.y == maxY)
            {
                endOfRight = true;
            }
            else
            {
                right = 0;
                for (int startIndex = robotPosition.y + 1; startIndex < maxY; startIndex++)
                {
                    if (_map[robotPosition.x, startIndex] != 1)
                    {
                        right++;
                    }
                    else
                    {
                        break;
                    }
                }
            }
        }

        private void getFront()
        {
            if (robotPosition.x == 0)
            {
                endOfFornt = true;
            }
            else
            {
                front = 0;

                for (int startIndex = robotPosition.x - 1; startIndex > 0; startIndex--)
                {
                    if (_map[startIndex, robotPosition.y] != 1)
                    {
                        front++;
                    }
                    else
                    {
                        break;
                    }
                }
            }
        }

        private void getBack()
        {
            if (robotPosition.x == maxX)
            {
                endOfBack = true;
            }
            else
            {
                back = 0;

                for (int startIndex = robotPosition.x + 1; startIndex < maxX; startIndex ++)
                {
                    if (_map[startIndex, robotPosition.y] != 1)
                    {
                        back++;
                    }
                    else
                    {
                        break;
                    }
                }
            }
        }



    }
}
