using System;
using System.Collections.Generic;
using System.Linq;

namespace IOANNIS
{
    public class GeneticAlgorithm
    {
        public const int setPopulationSize = 20;
        static Random rand = new Random();

        public static List<double[]> GenerateWeightPopulation()
        {

            List<double[]> population = new List<double[]>();

            for(int populationSize = 0; populationSize < setPopulationSize; populationSize++)
            {
                population.Add( new double[] { rand.NextDouble(), rand.NextDouble(), rand.NextDouble() } );
            }

            return population;
        }

        // Get the first half of ones with the best fitness 
        public static List<Chromosome> RankPopulation(List<Chromosome> listToEval)
        {
            var ordered = listToEval.OrderBy(chr => chr.fitness);

            List<Chromosome> newPopulation = new List<Chromosome>();

            for (int i = 0; i < setPopulationSize / 2; i++)
            {
                newPopulation.Add(ordered.ElementAt(i));
            }

            return newPopulation;
        }

        public static List<Chromosome> CreateNewGeneration(List<Chromosome> halfPopulation)
        {
            List<Chromosome> afterGeneration = new List<Chromosome>();

            for (int i = 0; i < halfPopulation.Count - 1; i++)
            {
                afterGeneration.Add(new Chromosome { weights = new double[] { halfPopulation.ElementAt(i).weights[0], halfPopulation.ElementAt(i + 1).weights[1], halfPopulation.ElementAt(i).weights[2] }, fitness = 0 });
                afterGeneration.Add(new Chromosome { weights = new double[] { halfPopulation.ElementAt(i+1).weights[0], halfPopulation.ElementAt(i).weights[1], halfPopulation.ElementAt(i+1).weights[2] }, fitness = 0 });
            }

            return afterGeneration;
        }

        public static void Mutate(List<Chromosome> population)
        {
            foreach (Chromosome c in population)
            {
                c.weights[Program.randomNumber.Next(1, 3)] = Program.randomNumber.NextDouble(); 
            }
        }
    }
    // left, right , front
    // direction = randomWeigth * left + randomWeight * right + front * randomWeight;
    // movement = randomWeigth * left + randomWeight * right + front * randomWeight;

    public class NeuralNetwork
    {
        public static double GenerateInstruction(double[] weights, SensorReadings sensorReadings)
        {
            return weights[0] * sensorReadings.front + weights[1] * sensorReadings.left + weights[2] * sensorReadings.right;
        }
    }

    public class Chromosome
    {
        public double[] weights;
        public int fitness; 
    }
}
