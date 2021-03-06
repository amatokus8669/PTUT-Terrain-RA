using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Assets.Scripts.lib
{
    class Terrain
    {
        private int CHUNK_SIZE = 32;

        private GameObject topArrow, leftArrow, bottomArrow, rightArrow; //fl�ches directionelles au bord du terrain
        
        public GameObject TerrainGameObject { get; set; }
        public int Id { get; set; } // Id = -1 not in database
        public string Nom { get; set; }
        public int Size { get; set; }
        public List<Chunk> ChunkList { get; set; }
        public int offsetX { get; set; } //West ++  East --
        public int offsetY { get; set; } //North ++ South --
        
        public List<int> VisibleChunksId { get;  set; }
        

        public Terrain(GameObject associatedGameObject, int size)
        {
            VisibleChunksId = new List<int>();
            offsetX = 0;
            offsetY = 0;
            TerrainGameObject = associatedGameObject;
            Size = size;
            ChunkList = new List<Chunk>();
            generateNewTerrain();
            placeArrows();
            Id = -1;
            initVisibleChunksList();
        }

        public Terrain(IDbConnection dbConnection, GameObject associatedGameObject, int id)
        {
            VisibleChunksId = new List<int>();
            offsetX = 0;
            offsetY = 0;
            TerrainGameObject = associatedGameObject;
            Id = id;
            ChunkList = new List<Chunk>();
            load(dbConnection);
            placeArrows();
            initVisibleChunksList();
        }

        public void DestroyTerrain()
        {
            foreach(Chunk c in ChunkList)
            {
                GameObject.Destroy(c.ChunkGameObject);
            }
            ChunkList.Clear();
        }
        private void initVisibleChunksList()
        {
            foreach(Chunk c in ChunkList)
            {
                if (c.Visible)
                {
                    VisibleChunksId.Add(c.Id);
                }
            }
        }

        private void generateNewTerrain()
        {
            for(int i = 0; i<Size*Size; i++)
            {
                Chunk c = new Chunk(this, i, CHUNK_SIZE);
                ChunkList.Add(c);
            }

        }

        private void placeArrows()
        {
            leftArrow = TerrainGameObject.transform.GetChild(1).gameObject;
            leftArrow.transform.position = new Vector3(-10f, 0f, -16f);

            rightArrow = TerrainGameObject.transform.GetChild(2).gameObject;
            rightArrow.transform.position = new Vector3(136f, 0f, 16f);

            topArrow = TerrainGameObject.transform.GetChild(3).gameObject;
            topArrow.transform.position = new Vector3(47.9f, 0f, 72f);

            bottomArrow = TerrainGameObject.transform.GetChild(4).gameObject;
            bottomArrow.transform.position = new Vector3(80.1f, 0f, -72f);
        }

        public void recalculateAdjacentHeights(Chunk chunk)
        {
            List<Vector3> centerChunkHeights = new List<Vector3>();
            chunk.ChunkGameObject.GetComponent<MeshFilter>().mesh.GetVertices(centerChunkHeights);

            //Bord inf�rieur
            Chunk bottomChunk = chunk.getBottomChunk();
            if (bottomChunk != null)
            {
                List<Vector3> adjacentChunkHeights = new List<Vector3>();
                bottomChunk.ChunkGameObject.GetComponent<MeshFilter>().mesh.GetVertices(adjacentChunkHeights);
                for (int i = 0; i <= CHUNK_SIZE; i++)
                {
                    Vector3 vertice = adjacentChunkHeights[(CHUNK_SIZE + 1) * CHUNK_SIZE + i];
                    adjacentChunkHeights[(CHUNK_SIZE + 1) * CHUNK_SIZE + i] = new Vector3(vertice.x, centerChunkHeights[i].y, vertice.z);
                }
                bottomChunk.ChunkGameObject.GetComponent<MeshFilter>().mesh.SetVertices(adjacentChunkHeights);
            }

            //Bord sup�rieur
            Chunk topchunk = chunk.getTopChunk();
            if (topchunk != null)
            {
                List<Vector3> adjacentChunkHeights = new List<Vector3>();
                topchunk.ChunkGameObject.GetComponent<MeshFilter>().mesh.GetVertices(adjacentChunkHeights);
                for (int i = 0; i <= CHUNK_SIZE; i++)
                {
                    Vector3 vertice = adjacentChunkHeights[i];
                    adjacentChunkHeights[i] = new Vector3(vertice.x, centerChunkHeights[(CHUNK_SIZE + 1) * CHUNK_SIZE + i].y, vertice.z);
                }
                topchunk.ChunkGameObject.GetComponent<MeshFilter>().mesh.SetVertices(adjacentChunkHeights);
            }

            //Bord gauche
            Chunk leftchunk = chunk.getLeftChunk();
            if (leftchunk != null)
            {
                List<Vector3> adjacentChunkHeights = new List<Vector3>();
                leftchunk.ChunkGameObject.GetComponent<MeshFilter>().mesh.GetVertices(adjacentChunkHeights);
                for (int i = 0; i <= (CHUNK_SIZE + 1) * CHUNK_SIZE; i += CHUNK_SIZE + 1)
                {
                    Vector3 vertice = adjacentChunkHeights[i + CHUNK_SIZE];
                    adjacentChunkHeights[i + CHUNK_SIZE] = new Vector3(vertice.x, centerChunkHeights[i].y, vertice.z);
                }
                leftchunk.ChunkGameObject.GetComponent<MeshFilter>().mesh.SetVertices(adjacentChunkHeights);
            }


            //Bord Droit
            Chunk rightchunk = chunk.getRightChunk();
            if (rightchunk != null)
            {
                List<Vector3> adjacentChunkHeights = new List<Vector3>();
                rightchunk.ChunkGameObject.GetComponent<MeshFilter>().mesh.GetVertices(adjacentChunkHeights);
                for (int i = 0; i <= (CHUNK_SIZE + 1) * CHUNK_SIZE; i += CHUNK_SIZE + 1)
                {
                    Vector3 vertice = adjacentChunkHeights[i];
                    adjacentChunkHeights[i] = new Vector3(vertice.x, centerChunkHeights[i + CHUNK_SIZE].y, vertice.z);
                }
                rightchunk.ChunkGameObject.GetComponent<MeshFilter>().mesh.SetVertices(adjacentChunkHeights);
            }

            //Coin inf�rieur gauche
            Chunk bottomLeftChunk = chunk.getBottomLeftChunk();
            if (bottomLeftChunk != null)
            {
                List<Vector3> adjacentChunkHeights = new List<Vector3>();
                bottomLeftChunk.ChunkGameObject.GetComponent<MeshFilter>().mesh.GetVertices(adjacentChunkHeights);
                Vector3 vertice = adjacentChunkHeights[(CHUNK_SIZE + 1) * (CHUNK_SIZE + 1) - 1];
                adjacentChunkHeights[(CHUNK_SIZE + 1) * (CHUNK_SIZE + 1) - 1] = new Vector3(vertice.x, centerChunkHeights[0].y, vertice.z);
                bottomLeftChunk.ChunkGameObject.GetComponent<MeshFilter>().mesh.SetVertices(adjacentChunkHeights);
            }

            //Coin inf�rieur droit
            Chunk bottomRightChunk = chunk.getBottomRightChunk();
            if (bottomRightChunk != null)
            {
                List<Vector3> adjacentChunkHeights = new List<Vector3>();
                bottomRightChunk.ChunkGameObject.GetComponent<MeshFilter>().mesh.GetVertices(adjacentChunkHeights);
                Vector3 vertice = adjacentChunkHeights[(CHUNK_SIZE + 1) * CHUNK_SIZE];
                adjacentChunkHeights[(CHUNK_SIZE + 1) * CHUNK_SIZE] = new Vector3(vertice.x, centerChunkHeights[CHUNK_SIZE].y, vertice.z);
                bottomRightChunk.ChunkGameObject.GetComponent<MeshFilter>().mesh.SetVertices(adjacentChunkHeights);
            }

            //Coin sup�rieur gauche
            Chunk topLeftChunk = chunk.getTopRightChunk();
            if (topLeftChunk != null)
            {
                List<Vector3> adjacentChunkHeights = new List<Vector3>();
                topLeftChunk.ChunkGameObject.GetComponent<MeshFilter>().mesh.GetVertices(adjacentChunkHeights);
                Vector3 vertice = adjacentChunkHeights[0];
                adjacentChunkHeights[0] = new Vector3(vertice.x, centerChunkHeights[(CHUNK_SIZE + 1) * (CHUNK_SIZE + 1) - 1].y, vertice.z);
                topLeftChunk.ChunkGameObject.GetComponent<MeshFilter>().mesh.SetVertices(adjacentChunkHeights);
            }

            //Coin sup�rieur droit
            Chunk topRightChunk = chunk.getTopLeftChunk();
            if (topRightChunk != null)
            {
                List<Vector3> adjacentChunkHeights = new List<Vector3>();
                topRightChunk.ChunkGameObject.GetComponent<MeshFilter>().mesh.GetVertices(adjacentChunkHeights);
                Vector3 vertice = adjacentChunkHeights[CHUNK_SIZE];
                adjacentChunkHeights[CHUNK_SIZE] = new Vector3(vertice.x, centerChunkHeights[(CHUNK_SIZE + 1) * CHUNK_SIZE].y, vertice.z);
                topRightChunk.ChunkGameObject.GetComponent<MeshFilter>().mesh.SetVertices(adjacentChunkHeights);
            }
        }

        public void moveNorth()
        {
            if(Size> 4 && offsetY < Size - 4)
            {
                offsetY++;
                foreach (Chunk c in ChunkList)
                {
                    c.moveNorth();
                }
            }
        }
        public void moveSouth()
        {
            if (Size > 4 && offsetY > 0)
            {
                offsetY--;
                foreach (Chunk c in ChunkList)
                {
                    c.moveSouth();
                }
            }
        }
        public void moveEast()
        {
            if (Size > 4 && offsetX > 0)
            {
                offsetX--;
                foreach (Chunk c in ChunkList)
                {
                    c.moveEast();
                }
            }
        }
        public void moveWest()
        {
            if (Size > 4 && offsetX < Size - 4)
            {
                offsetX++;
                foreach (Chunk c in ChunkList)
                {
                    c.moveWest();
                }
            }
        }

        public void recalculateVisibility()
        {
            foreach (Chunk c in ChunkList)
            {
                if (VisibleChunksId.Contains(c.Id))
                {
                    c.Visible = true;
                }
                else
                {
                    c.Visible = false;
                }
            }
        }


        #region Database
        #region Save
        public void save(IDbConnection dbConnection)
        {
            if(Id == -1)
            {
                //Nouvel enregistrement dans la BD
                insertTerrainData(dbConnection);
            }
            else
            {
                updateTerrainData(dbConnection);
            }
        }

        private void insertTerrainData(IDbConnection dbConnection) //Utilis� quand le terrain n'a encore jamais �t� suvegard�
        {
            IDbCommand command = dbConnection.CreateCommand();
            string sqlInsertCommand = "INSERT INTO 'Terrain' VALUES (null,'" + Nom + "'," + Size + ");";
            command.CommandText = sqlInsertCommand;
            command.ExecuteNonQuery();
            command.Dispose();

            command = dbConnection.CreateCommand();
            string sqlIdOfLastInsert = "SELECT terrainId from 'Terrain' ORDER BY terrainID DESC LIMIT 1;";
            command.CommandText = sqlIdOfLastInsert;
            IDataReader reader = command.ExecuteReader();
            reader.Read();
            Id = reader.GetInt32(0);
            command.Dispose();

            foreach (Chunk chunk in ChunkList)
            {
                Debug.Log("Start save Chunk " + chunk.Id);
                chunk.save(dbConnection);
                Debug.Log("End save Chunk " + chunk.Id);
            }
            Debug.Log("Insert Terrain with id: " + Id);
        }

        public void updateTerrainData(IDbConnection dbConnection)
        {
            clearData(dbConnection);
            foreach(Chunk chunk in ChunkList)
            {
                chunk.save(dbConnection);
            }
        }

        private void clearData(IDbConnection dbConnection)
        {

            IDbCommand command = dbConnection.CreateCommand();
            string sqlDeleteCommand = "DELETE FROM 'Chunk' WHERE terrainID = " + Id + ";";
            command.CommandText = sqlDeleteCommand;
            command.ExecuteNonQuery();
            command.Dispose();

            command = dbConnection.CreateCommand();
            sqlDeleteCommand = "DELETE FROM 'HeightMap' WHERE terrainID = " + Id + ";";
            command.CommandText = sqlDeleteCommand;
            command.ExecuteNonQuery();
            command.Dispose();

            command = dbConnection.CreateCommand();
            sqlDeleteCommand = "DELETE FROM 'Nature' WHERE terrainID = " + Id + ";";
            command.CommandText = sqlDeleteCommand;
            command.ExecuteNonQuery();
            command.Dispose();
        }
        #endregion

        #region Load
        private void load(IDbConnection dbConnection)
        {
            string sqlSelectCommand = "SELECT * FROM 'Terrain' WHERE terrainId = " + Id + ";";
            IDbCommand command = dbConnection.CreateCommand();
            command.CommandText = sqlSelectCommand;
            IDataReader reader = command.ExecuteReader();
            reader.Read();

            Nom = reader.GetString(1);
            Size = reader.GetInt32(2);
            command.Dispose();
            

            for(int i = 0; i<Size*Size; i++)
            {
                Chunk c = new Chunk(dbConnection, this, Id, Size, i);
                ChunkList.Add(c);
            }
        }
        #endregion
        #endregion
    }
}
