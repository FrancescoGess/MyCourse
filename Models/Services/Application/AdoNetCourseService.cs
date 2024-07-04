using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MyCourse.Models.Services.Infrastucture;
using MyCourse.Models.ViewModels;
using System.Data;

namespace MyCourse.Models.Services.Application
{
    public class AdoNetCourseService : ICourseService
    {
        //Questo servizio applicativo utilizza l'interfaccia IDatabaseAccessor per accedere al database
        private readonly IDatabaseAccessor db;

        public AdoNetCourseService(IDatabaseAccessor db){
            this.db = db;
        }

        //metodo che recupera la lista di tutti i corsi presenti nel database
        public List<CourseViewModel> GetCourses()
        {
            //query che verrà eseguita nel database
            string query = "SELECT Id, Title, ImagePath, Author,Rating, FullPrice_Amount, FullPrice_Currency, CurrentPrice_Amount, CurrentPrice_Currency FROM Courses";
            //un oggetto di tipo DataSet è un insieme di oggetti di tipo DataTable
            DataSet dataSet = db.Query(query); 

            //Un dataTable è una tabella in cui vengono memorizzati i dati recuperati da una SELECT nel db
            //dato che in un dataSet possono esserci più tabelle, con l'indice accedo all'i-esima tabella
            var dataTable = dataSet.Tables[0];
            var courseList = new List<CourseViewModel>();
            //scorro l'oggetto DataTable riga per riga tramite la proprietà Rows
            //per ogni riga (oggetto DataRow) leggi i dati e crea l'oggetto CourseViewModel
            foreach(DataRow courseRow in dataTable.Rows){
                var course = CourseViewModel.FromDataRow(courseRow);
                courseList.Add(course);
            }
            return courseList;
        }

        //metodo che deve eseguire due query
        //la prima deve recuperare da db tutti i dati del corso con lo specifico id
        //le seconda deve recuperare tutte le lezioni associate al corso con lo specifico id
        public CourseDetailViewModel GetCourse(int id)
        {
            //In un'unica variabile string io inserisco tutte le query che devono essere eseguite
            string query = "SELECT Id, Title, Description, ImagePath, Author,Rating, FullPrice_Amount, FullPrice_Currency, CurrentPrice_Amount, CurrentPrice_Currency FROM Courses WHERE Id =" 
            + id + "; SELECT Id, Title, Description, Duration FROM Lessons WHERE CourseId =" + id;
            //in questo dataSet ci saranno due tabelle: la prima con i dati del corso e la seconda con i dati delle lezioni del corso
            DataSet dataSet = db.Query(query);
            var courseDataTable = dataSet.Tables[0];//accedo dal dataSet alla prima tabella cioè a quella che è stata restituita dall'esecuzione dell aprima query
            if(courseDataTable.Rows.Count != 1){//sto controllando se la tabella ha recuperato esattamente un dato/corso
                throw new InvalidOperationException($"Corso con id = {id} non trovato");
            }
            var courseRow = courseDataTable.Rows[0];//accedo alla prima riga della tabella
            var courseDetailViewModel = CourseDetailViewModel.FromDataRow(courseRow);
            
            
            var lessonDataTable = dataSet.Tables[1];//accedo dal dataSet alla seconda tabella cioè a quella che è stata restituita dall'esecuzione della seconda query
            foreach(DataRow lessonRow in lessonDataTable.Rows){
                var lesson = LessonViewModel.FromDataRow(lessonRow);
                courseDetailViewModel.Lessons.Add(lesson);
            }

            return courseDetailViewModel;
        }


    }
}