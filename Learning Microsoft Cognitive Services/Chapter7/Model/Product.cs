namespace Chapter7.Model
{
    /// <summary>
    /// Model for a given product
    /// </summary>
    public class Product
    {
        /// <summary>
        /// ID property for a product
        /// </summary>
        public string Id { get; set; }
        /// <summary>
        /// The name of the product
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// The product category
        /// </summary>
        public string Category { get; set; }

        /// <summary>
        /// Create a Product object
        /// </summary>
        /// <param name="id">String param for unique product ID</param>
        /// <param name="name">String param for product name</param>
        /// <param name="category">String param for product category</param>
        public Product(string id, string name, string category)
        {
            Id = id;
            Name = name;
            Category = category;
        }
    }
}