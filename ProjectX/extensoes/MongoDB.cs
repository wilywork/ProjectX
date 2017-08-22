//////using System;
//////using System.Data;
//////using System.Collections.Generic;
//////using System.Linq;
//////using System.Text;
//////using MongoDB.Bson;
//////using MongoDB.Driver;
//////using Fougerite;

//////namespace ProjectX.extensoes
//////{
//////    public class MongoDB
//////    {

//////        public static MongoClient conn;

//////        public static void connect(string connection)
//////        {
//////            conn = new MongoClient(connection);
//////        }


//////    //"steamId": "76561197964078813",
//////    //"kill": 10,
//////    //"death": 7,
//////    //"banned": false,
//////    //"id": "5860831c9b0528f001ba1d5a"

//////        public class playerRank
//////        {
//////            public ObjectId id { get; set; }
//////            public string steamId { get; set; }
//////            public int kill { get; set; }
//////            public int death { get; set; }
//////            public int banned { get; set; }
//////        }

//////        public static void InsertKill(string steamId, int kill, int death)
//////        {
//////            try
//////            {
//////                string connectionString = "mongodb://localhost:27017";
//////                var client = new MongoClient(connectionString);
//////                var server = client.GetServer();
//////                var db = server.GetDatabase("db");

//////                var rankDB = db.GetCollection<playerRank>("rank");

//////                playerRank playerKill = new playerRank();
//////                playerKill.steamId = steamId;
//////                playerKill.kill = kill;
//////                playerKill.kill = death;
//////                rankDB.Insert(playerKill);

//////                ObjectId _idInclusao = playerKill.id;
//////                UnityEngine.Debug.Log("Inseriu o registro: " + _idInclusao.ToString());
//////            }
//////            catch (Exception ex)
//////            {
//////                UnityEngine.Debug.Log(ex);
//////            }

//////        }

//////        //////insert("dadosDB","clientes", objCliente);
//////        ////public void insert<T>(T contact, string database, string table)
//////        ////{
//////        ////    IMongoDatabase db = conn.GetDatabase(database);
//////        ////    var contactsCollection = db.GetCollection<T>(table);
//////        ////    var saveobj = contact;
//////        ////    contactsCollection.Insert(saveobj);
//////        ////}

//////        ////public void GetCollection()
//////        ////{
//////        ////    MongoServer server = MongoServer.Create(connectionString);

//////        ////    MongoDatabase database = server.GetDatabase("MongoDBNet");

//////        ////    var contactsCollection = database.GetCollection<Contact>("Contacts");
//////        ////}

//////        ////public void UpdateContact(Contact contact)
//////        ////{
//////        ////    MongoServer server = MongoServer.Create(connectionString);

//////        ////    MongoDatabase database = server.GetDatabase("MongoDBNet");

//////        ////    var contactsCollection = database.GetCollection<Contact>("Contacts");

//////        ////    contactsCollection.Save(contact);
//////        ////}

//////        ////public void DeleteContact(Contact contact)
//////        ////{
//////        ////    MongoServer server = MongoServer.Create(connectionString);

//////        ////    MongoDatabase database = server.GetDatabase("MongoDBNet");

//////        ////    var contactsCollection = database.GetCollection<Contact>("Contacts");

//////        ////    var query = Query.EQ("_id", contact.Id);
//////        ////    contactsCollection.Remove(query);
//////        ////}

//////        ////public IEnumerable<Contact> GetContactsByName(string name)
//////        ////{
//////        ////    MongoServer server = MongoServer.Create(connectionString);

//////        ////    MongoDatabase database = server.GetDatabase("MongoDBNet");

//////        ////    var contactsCollection = database.GetCollection<Contact>("Contacts");

//////        ////    var query = from e in contactsCollection.AsQueryable<Contact>()
//////        ////                where e.Name == name
//////        ////                select e;

//////        ////    return query;
//////        ////}

//////    }
//////}


//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;

//using MongoDB.Bson;
//using MongoDB.Driver;
//using MongoDB.Driver.Builders;

//namespace ConsoleApplication1
//{
//    public class Entity
//    {
//        public ObjectId Id { get; set; }
//        public string Name { get; set; }
//    }

//    class Program
//    {
//        public static MongoClient client;
//        public static MongoServer server;
//        public static MongoDatabase database;
//        public static MongoCollection<Entity> collection;
//        public static string connectionString = "mongodb://127.0.0.1";


//        public static void Connectar()
//        {
//            try
//            {
//                if(server == null)
//                {
//                    client = new MongoClient(connectionString); //"mongodb://admin:admin@127.0.0.1:27017/db?ssl=true&authSource=admin"); //connectionString);//"
//                    server = client.GetServer();
//                    // var client = new MongoClient("mongodb://admin:admin@127.0.0.1:27017/admin?ssl=true&authSource=admin");
//                    // var database = client.GetDatabase("test");

//                    server.Connect();
//                    database = server.GetDatabase("db");
//                    collection = database.GetCollection<Entity>("rank");
//                    UnityEngine.Debug.Log("Banco conectado!");
//                }

//            }
//            catch (Exception ex)
//            {
//                UnityEngine.Debug.Log("FALHA AO CONECTAR: " + ex);
//            }
//        }

//        public static void Main()
//        {
//            try
//            {
//                Connectar();

//                var document = new BsonDocument {
//                    { "steamId", "76561197964078813" },
//                    { "kill", 10 },
//                    { "death", 7 },
//                    { "banned", false }
//                };
//                collection.Insert(document);



//                //if (collection != null)
//                //{
//                //    var entity = new Entity { Name = "Tom" };
//                //    collection.Insert(entity);
//                //    var id = entity.Id;
//                //    UnityEngine.Debug.Log("Inseriu o registro: " + id.ToString());
//                //    var query = Query<Entity>.EQ(e => e.Id, id);
//                //    entity = collection.FindOne(query);

//                //    entity.Name = "Dick";
//                //    collection.Save(entity);

//                //    var update = Update<Entity>.Set(e => e.Name, "Harry");
//                //    collection.Update(query, update);

//                //    collection.Remove(query);
//                //}
//            }
//            catch (Exception ex)
//            {
//                UnityEngine.Debug.Log(ex);
//            }

//        }

//    }
//}



using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Builders;

namespace ConsoleApplication1
{
    public class Entity
    {
        public ObjectId Id { get; set; }
        public string Name { get; set; }
    }

    class Program
    {
        public static void Main()
        {
            try
            {
                var connectionString = "mongodb://localhost";
                var client = new MongoClient(connectionString);
                var server = client.GetServer();
                server.Connect();
                //var database = server.GetDatabase("test");
                //var collection = database.GetCollection<Entity>("entities");

                //var entity = new Entity { Name = "Tom" };
                //collection.Insert(entity);
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.Log(ex);
            }

            //var id = entity.Id;

            //var query = Query<Entity>.EQ(e => e.Id, id);
            //entity = collection.FindOne(query);

            //entity.Name = "Dick";
            //collection.Save(entity);

            //var update = Update<Entity>.Set(e => e.Name, "Harry");
            //collection.Update(query, update);

            //collection.Remove(query);
        }
    }
}

public class mongo : ConsoleSystem
{
    [Admin]
    public static void insert(ref ConsoleSystem.Arg arg)
    {
        // ConsoleApplication1.Program.Main();
    }
}