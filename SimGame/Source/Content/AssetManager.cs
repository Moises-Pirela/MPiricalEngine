using System;
using System.IO;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using MPirical.Core.ECS;
using MPirical.Core.Serialization;

namespace MPirical.Content
{
    /// <summary>
    /// Asset manager for loading and managing game assets
    /// </summary>
    public class AssetManager
    {
        private ContentManager _content;
        private World _world;
        private EntitySerializer _entitySerializer;
        
        private Dictionary<string, Texture2D> _textures = new Dictionary<string, Texture2D>();
        private Dictionary<string, Model> _models = new Dictionary<string, Model>();
        private Dictionary<string, Effect> _effects = new Dictionary<string, Effect>();
        
        /// <summary>
        /// Create a new asset manager
        /// </summary>
        /// <param name="content">Content manager</param>
        /// <param name="world">ECS world</param>
        public AssetManager(ContentManager content, World world)
        {
            _content = content;
            _world = world;
            _entitySerializer = new EntitySerializer(world);
        }
        
        /// <summary>
        /// Load all game assets
        /// </summary>
        public void LoadAllAssets()
        {
            // Load textures
            LoadTextures();
            
            // Load models
            LoadModels();
            
            // Load effects
            LoadEffects();
        }
        
        /// <summary>
        /// Load all textures
        /// </summary>
        private void LoadTextures()
        {
            // In a real implementation, we might scan a directory or use a manifest file
            // For now, we'll hardcode some common textures
            try
            {
                _textures["default"] = _content.Load<Texture2D>("Textures/default");
                _textures["floor"] = _content.Load<Texture2D>("Textures/floor");
                _textures["wall"] = _content.Load<Texture2D>("Textures/wall");
                _textures["door"] = _content.Load<Texture2D>("Textures/door");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading textures: {ex.Message}");
                
                // Create fallback textures
                CreateFallbackTextures();
            }
        }
        
        /// <summary>
        /// Create fallback textures if loading fails
        /// </summary>
        private void CreateFallbackTextures()
        {
            // Create a simple checkerboard texture as a fallback
            Texture2D fallbackTexture = CreateCheckerboardTexture(64, 64, Color.Magenta, Color.Black);
            _textures["default"] = fallbackTexture;
            _textures["floor"] = fallbackTexture;
            _textures["wall"] = fallbackTexture;
            _textures["door"] = fallbackTexture;
        }
        
        /// <summary>
        /// Load all models
        /// </summary>
        private void LoadModels()
        {
            // In a real implementation, we would load model assets
            // For now, this is a placeholder
        }
        
        /// <summary>
        /// Load all effects (shaders)
        /// </summary>
        private void LoadEffects()
        {
            // In a real implementation, we would load custom shaders
            // For now, this is a placeholder
        }
        
        /// <summary>
        /// Create a checkerboard texture for placeholder visualization
        /// </summary>
        /// <param name="width">Texture width</param>
        /// <param name="height">Texture height</param>
        /// <param name="color1">First color</param>
        /// <param name="color2">Second color</param>
        /// <returns>A checkerboard texture</returns>
        private Texture2D CreateCheckerboardTexture(int width, int height, Color color1, Color color2)
        {
            Texture2D texture = new Texture2D(GameServices.GetService<GraphicsDevice>(), width, height);
            Color[] data = new Color[width * height];
            
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    bool isEvenRow = (y / 8) % 2 == 0;
                    bool isEvenCol = (x / 8) % 2 == 0;
                    
                    // XOR to create checkerboard pattern
                    bool useColor1 = isEvenRow ^ isEvenCol;
                    
                    data[y * width + x] = useColor1 ? color1 : color2;
                }
            }
            
            texture.SetData(data);
            return texture;
        }
        
        /// <summary>
        /// Get a texture by name
        /// </summary>
        /// <param name="textureName">Name of the texture</param>
        /// <returns>The requested texture</returns>
        public Texture2D GetTexture(string textureName)
        {
            if (_textures.TryGetValue(textureName, out Texture2D texture))
            {
                return texture;
            }
            
            // Return default texture if not found
            return _textures["default"];
        }
        
        /// <summary>
        /// Get a model by name
        /// </summary>
        /// <param name="modelName">Name of the model</param>
        /// <returns>The requested model</returns>
        public Model GetModel(string modelName)
        {
            if (_models.TryGetValue(modelName, out Model model))
            {
                return model;
            }
            
            // Return null if not found (caller should check)
            return null;
        }
        
        /// <summary>
        /// Get an effect by name
        /// </summary>
        /// <param name="effectName">Name of the effect</param>
        /// <returns>The requested effect</returns>
        public Effect GetEffect(string effectName)
        {
            if (_effects.TryGetValue(effectName, out Effect effect))
            {
                return effect;
            }
            
            // Return null if not found (caller should check)
            return null;
        }
        
        /// <summary>
        /// Load a level from file
        /// </summary>
        /// <param name="levelName">Name of the level file</param>
        /// <returns>List of entities in the level</returns>
        public List<Entity> LoadLevel(string levelName)
        {
            try
            {
                // Check for level file in content directory
                string levelPath = Path.Combine(_content.RootDirectory, "Levels", $"{levelName}.json");
                
                // If level file exists, load entities from it
                if (File.Exists(levelPath))
                {
                    return _entitySerializer.LoadEntitiesFromFile(levelPath);
                }
                
                // If level file doesn't exist, return an empty list
                Console.WriteLine($"Level file not found: {levelPath}");
                return new List<Entity>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading level {levelName}: {ex.Message}");
                return new List<Entity>();
            }
        }
        
        /// <summary>
        /// Save a level to file
        /// </summary>
        /// <param name="levelName">Name of the level file</param>
        /// <param name="entities">Entities to save</param>
        public void SaveLevel(string levelName, IEnumerable<Entity> entities)
        {
            try
            {
                // Ensure directory exists
                string levelDirectory = Path.Combine(_content.RootDirectory, "Levels");
                Directory.CreateDirectory(levelDirectory);
                
                // Save entities to file
                string levelPath = Path.Combine(levelDirectory, $"{levelName}.json");
                _entitySerializer.SaveEntitiesToFile(entities, levelPath);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving level {levelName}: {ex.Message}");
            }
        }
    }
    
    /// <summary>
    /// Game services provider for accessing global services
    /// </summary>
    public static class GameServices
    {
        private static GameServiceContainer _container = new GameServiceContainer();
        
        /// <summary>
        /// Add a service to the container
        /// </summary>
        /// <typeparam name="T">Service type</typeparam>
        /// <param name="service">Service instance</param>
        public static void AddService<T>(T service)
        {
            _container.AddService(typeof(T), service);
        }
        
        /// <summary>
        /// Get a service from the container
        /// </summary>
        /// <typeparam name="T">Service type</typeparam>
        /// <returns>Service instance</returns>
        public static T GetService<T>()
        {
            return (T)_container.GetService(typeof(T));
        }
    }
}