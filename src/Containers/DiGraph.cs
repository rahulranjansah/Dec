/**
 * A generic directed graph (DiGraph) implementation backed by an adjacency list.
 * Vertices are stored as keys, and each vertex maps to a DLL<T> representing its outgoing edges.
 *
 * Bugs: Potential inefficiency due to repeated .Keys.Contains lookups; relies on DLL<T>
 *       to provide correct behavior for adjacency operations.
 *
 * @author Rahul, Rick, Zach
 * @date 2025-11-20
 */

using System;

namespace Containers
{
    /// <summary>
    /// Represents a directed graph using an adjacency list backed by a dictionary.
    /// Provides methods for adding/removing vertices, adding/removing edges, and
    /// querying graph structure such as neighbors, vertex count, and edge count.
    /// </summary>
    /// <typeparam name="T">Vertex type (must be non-nullable)</typeparam>
    public class DiGraph<T> where T : notnull
    {
        /// <summary>
        /// Internal adjacency list where each vertex maps to a doubly linked list of neighbors.
        /// </summary>
        protected Dictionary<T, DLL<T>> _adjacencyList;

        /// <summary>
        /// Initializes an empty directed graph.
        /// </summary>
        public DiGraph()
        {
            // Create empty dictionary to store vertices and adjacency lists
            this._adjacencyList = new Dictionary<T, DLL<T>>();
        }

        /// <summary>
        /// Adds a vertex to the graph.
        /// </summary>
        /// <param name="vertex">Vertex to add</param>
        /// <returns>False if the vertex already exists; true otherwise</returns>
        public bool AddVertex(T vertex)
        {
            // Avoid adding duplicates; dictionary keys must remain unique
            if (_adjacencyList.Keys.Contains(vertex))
            {
                return false;
            }

            // Insert new vertex with an empty adjacency list
            _adjacencyList.Add(vertex, new DLL<T>());
            return true;
        }

        /// <summary>
        /// Adds a directed edge from source to destination.
        /// </summary>
        /// <param name="source">Starting vertex</param>
        /// <param name="destination">Target vertex</param>
        /// <returns>False if edge already exists; true if added</returns>
        /// <exception cref="ArgumentException">Thrown if either vertex is not in the graph</exception>
        public bool AddEdge(T source, T destination)
        {
            // Validate both vertices exist
            if (!_adjacencyList.Keys.Contains(source) || !_adjacencyList.Keys.Contains(destination))
            {
                throw new ArgumentException($"Node {source} or {destination} not in DiGraph");
            }

            // Prevent duplicate edges
            if (_adjacencyList[source].Contains(destination)) return false;

            // Add directed edge
            _adjacencyList[source].Add(destination);
            return true;
        }

        /// <summary>
        /// Removes a vertex and all edges pointing to it.
        /// </summary>
        /// <param name="vertex">Vertex to remove</param>
        /// <returns>False if vertex not found; true if removed</returns>
        public bool RemoveVertex(T vertex)
        {
            // Validate vertex exists
            if (!_adjacencyList.Keys.Contains(vertex)) return false;

            // Remove all inbound edges referencing vertex
            foreach (T node in _adjacencyList.Keys)
            {
                if (_adjacencyList[node].Contains(vertex))
                {
                    _adjacencyList[node].Remove(vertex);
                }
            }

            // Remove vertex and its adjacency list
            return _adjacencyList.Remove(vertex);
        }

        /// <summary>
        /// Removes a directed edge from source to destination.
        /// </summary>
        /// <param name="source">Starting vertex</param>
        /// <param name="destination">Target vertex</param>
        /// <returns>False if edge does not exist; true if removed</returns>
        /// <exception cref="ArgumentException">Thrown if source or destination vertex is missing</exception>
        public bool RemoveEdge(T source, T destination)
        {
            // Validate vertices
            if (!_adjacencyList.Keys.Contains(source) || !_adjacencyList.Keys.Contains(destination))
            {
                throw new ArgumentException($"Node {source} or {destination} not in DiGraph");
            }

            // Edge not present → nothing to remove
            if (!_adjacencyList[source].Contains(destination)) return false;

            // Remove directed edge
            return _adjacencyList[source].Remove(destination);
        }

        /// <summary>
        /// Checks whether a directed edge exists from source to destination.
        /// </summary>
        /// <param name="source">Starting vertex</param>
        /// <param name="destination">Target vertex</param>
        /// <returns>True if the edge exists; false otherwise</returns>
        public bool HasEdge(T source, T destination)
        {
            // If the source vertex is missing, the edge can't exist
            if (!_adjacencyList.Keys.Contains(source)) return false;

            return _adjacencyList[source].Contains(destination);
        }

        /// <summary>
        /// Returns all neighbors that can be reached directly from the given vertex.
        /// </summary>
        /// <param name="vertex">Vertex whose adjacency list is requested</param>
        /// <returns>A list of adjacent vertices</returns>
        /// <exception cref="ArgumentException">Thrown if vertex does not exist</exception>
        public List<T> GetNeighbors(T vertex)
        {
            // Ensure vertex exists
            if (!_adjacencyList.Keys.Contains(vertex))
            {
                throw new ArgumentException($"{vertex} not found in DiGraph");
            }

            // Extract neighbors from stored DLL<T>
            List<T> AdjacentNodes = new List<T>();
            foreach (T node in _adjacencyList[vertex])
            {
                AdjacentNodes.Add(node);
            }
            return AdjacentNodes;
        }

        /// <summary>
        /// Returns all vertices stored in the graph.
        /// </summary>
        /// <returns>Enumerable sequence of vertices</returns>
        public IEnumerable<T> GetVertices()
        {
            // Yield each key from adjacency list dictionary
            foreach (T node in _adjacencyList.Keys)
            {
                yield return node;
            }
        }

        /// <summary>
        /// Returns the total number of vertices in the graph.
        /// </summary>
        public int VertexCount()
        {
            return _adjacencyList.Count();
        }

        /// <summary>
        /// Computes the total number of directed edges in the graph.
        /// </summary>
        public int EdgeCount()
        {
            int count = 0;

            // Sum all adjacency lists to get edge count
            foreach (T node in _adjacencyList.Keys)
            {
                count += _adjacencyList[node].Count;
            }

            return count;
        }

        /// <summary>
        /// Returns a high-level string summary of the graph's vertex and edge count.
        /// </summary>
        public string ToString()
        {
            return $"Vertices: {VertexCount()} Edges: {EdgeCount()}. \n {_adjacencyList.ToString()}";
        }
    }
}
