using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using System.Data;
using System.Data.SqlClient;

namespace ConsoleApp
{
    partial class Program
    {
        static void Main(string[] args)
        {
            using (StreamReader r = new("config.json"))
            {
                string json = r.ReadToEnd();
                dynamic config = JsonConvert.DeserializeObject(json);

                string ruta = config.ruta;
                string conexionStr = config.conexion;

                using SqlConnection conexion = new(conexionStr);
                conexion.Open();

                List<string> archivosSql = Directory.GetFiles(ruta)
                    .Where(file => file.EndsWith(".sql") && Regex.IsMatch(file, @"\d+\.-"))
                    .OrderBy(file => int.Parse(Regex.Match(file, @"\d+").Value))
                    .ToList();

                foreach (string archivoSql in archivosSql)
                {
                    string nombreArchivo = archivoSql.Split('-')[1].Trim();

                    string sql = File.ReadAllText(Path.Combine(ruta, archivoSql));

                    try
                    {
                        using SqlCommand command = new(sql, conexion);
                        command.ExecuteNonQuery();
                        Console.WriteLine($"Se ha aplicado el archivo SQL {nombreArchivo}.");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error al ejecutar el archivo SQL {nombreArchivo}: {ex.Message}");
                    }
                }

                conexion.Close();
                Console.WriteLine("Se han aplicado todos los archivos SQL.");
            }

            Console.ReadLine();
        }
    }
}
