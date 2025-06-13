public class LimsRAGSystem
{
    // --- 1. Simulate our "Knowledge Base" (in-memory for demonstration) ---
    // In a real scenario, this would be a vector database (e.g., Pinecone, Weaviate, Chroma)
    // storing embeddings and their associated text chunks.
    private readonly Dictionary<string, string> _knowledgeBase = new Dictionary<string, string>
    {
        {"sop_ph_meter_calibration_001", "SOP-001: pH Meter Calibration. Ensure instrument is clean. Use buffer solutions pH 4.0, 7.0, 10.0. Calibrate daily or before use. Record readings in logbook."},
        {"troubleshooting_hplc_low_signal", "HPLC Troubleshooting: Low signal can be caused by a dirty detector, clogged column, or improperly prepared mobile phase. Check lamp intensity and detector settings first. Flush column with appropriate solvent."},
        {"sample_prep_dna_extraction_002", "SOP-002: DNA Extraction from Blood. Centrifuge sample to separate plasma. Lyse cells with buffer AL. Incubate at 56°C. Use spin column for purification."},
        {"instrument_manual_spectrophotometer_agilent", "Agilent UV-Vis Spectrophotometer manual. Wavelength range: 190-1100 nm. Ensure cuvette path length is 1 cm. Clean optics regularly. Power cycle if no response."},
        {"lab_safety_guidelines_chemical_spills", "Lab Safety: For chemical spills, immediately contain the spill. Use appropriate PPE (gloves, goggles). Absorb with spill kit materials. Dispose according to SDS. Inform supervisor."},
        {"sample_results_batch_20250610_ph", "Batch ID 20250610: Sample A1 (pH 7.1), Sample A2 (pH 7.0), Sample B1 (pH 6.9). All within acceptable range. Calibrated pH meter prior to analysis."},
        {"lqts_workflow_sample_reception", "LIMS workflow for sample reception: Log sample ID, date, time, source. Assign unique LIMS number. Store at specified temperature. Verify sample integrity."}
    };

    // --- 2. Simulate an "Embedding Model" ---
    // In reality, this would be an API call to an embedding service (e.g., OpenAI, Hugging Face Sentence Transformers)
    // or a locally deployed model that generates high-dimensional vectors.
    // For simplicity, we'll just use a basic string hash as a "proxy" for an embedding.
    private int GenerateSimpleEmbedding(string text)
    {
        return text.ToLowerInvariant().GetHashCode();
    }

    // --- 3. Simulate "Similarity Search" (Retrieval) ---
    // In a real vector database, this would involve cosine similarity or other vector distance metrics.
    // Here, we're doing a very basic keyword search for demonstration.
    private List<string> RetrieveRelevantContext(string userQuery, int topK = 3)
    {
        var queryKeywords = userQuery.ToLowerInvariant().Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)
                                     .Distinct().ToList();

        var relevantChunks = new Dictionary<string, int>(); // chunk -> score (match count)

        foreach (var entry in _knowledgeBase)
        {
            var chunkText = entry.Value.ToLowerInvariant();
            int score = 0;
            foreach (var keyword in queryKeywords)
            {
                if (chunkText.Contains(keyword))
                {
                    score++;
                }
            }
            if (score > 0)
            {
                relevantChunks.Add(entry.Value, score);
            }
        }

        // Order by score descending and take top K
        return relevantChunks.OrderByDescending(x => x.Value)
                             .Select(x => x.Key)
                             .Take(topK)
                             .ToList();
    }

    // --- 4. Simulate "Large Language Model" (Generation) ---
    // In reality, this would be an API call to an LLM service.
    // It would take the user's query and the retrieved context to generate a coherent answer.
    private async Task<string> GenerateResponseAsync(string userQuery, List<string> retrievedContexts)
    {
        // For a real LLM, you'd construct a prompt like:
        /*
        "You are a helpful LIMS assistant. Use the following context to answer the user's question.
        If the information is not in the context, state that you don't have enough information.

        User Question: {userQuery}

        Context:
        {string.Join("\n\n", retrievedContexts)}

        Answer:"
        */

        Console.WriteLine("\n--- Simulating LLM Generation ---");
        Console.WriteLine($"User Query: {userQuery}");
        Console.WriteLine("Retrieved Contexts:");
        foreach (var context in retrievedContexts)
        {
            Console.WriteLine($"- {context}");
        }

        if (!retrievedContexts.Any())
        {
            return "I couldn't find any relevant information in my knowledge base for your query. Please try rephrasing or provide more details.";
        }

        // Dummy LLM: Just combines the query and context
        string combinedContext = string.Join("\n\n", retrievedContexts);
        string generatedAnswer = $"Based on the information I have, for your query about '{userQuery}':\n\n{combinedContext}\n\n(Note: This is a simplified response. A real LLM would synthesize and summarize.)";

        // Simulate API call latency
        await Task.Delay(500);
        return generatedAnswer;
    }

    // --- 5. The RAG Orchestration Flow ---
    public async Task<string> AskLimsRAG(string userQuery)
    {
        Console.WriteLine($"\nUser: {userQuery}");

        // Step 1: Retrieve relevant documents/chunks
        Console.WriteLine("--- Retrieving Context ---");
        var retrievedContexts = RetrieveRelevantContext(userQuery, topK: 2); // Get top 2 most relevant pieces of info

        if (!retrievedContexts.Any())
        {
            Console.WriteLine("No relevant context found.");
            return await GenerateResponseAsync(userQuery, new List<string>()); // Still try to generate, but with no context
        }

        // Step 2: Augment the query with the retrieved context and generate a response
        var finalAnswer = await GenerateResponseAsync(userQuery, retrievedContexts);

        return finalAnswer;
    }

    // --- Main method for demonstration ---
    public static async Task Main(string[] args)
    {
        LimsRAGSystem ragSystem = new LimsRAGSystem();

        Console.WriteLine("LIMS RAG System (Simplified Demo)");
        Console.WriteLine("---------------------------------\n");

        await ragSystem.AskLimsRAG("How do I calibrate the pH meter?");
        Console.WriteLine("\n---------------------------------");
        await ragSystem.AskLimsRAG("What causes low signal in HPLC?");
        Console.WriteLine("\n---------------------------------");
        await ragSystem.AskLimsRAG("Tell me about DNA extraction from blood.");
        Console.WriteLine("\n---------------------------------");
        await ragSystem.AskLimsRAG("What should I do for a chemical spill?");
        Console.WriteLine("\n---------------------------------");
        await ragSystem.AskLimsRAG("What are the results for batch 20250610 pH?");
        Console.WriteLine("\n---------------------------------");
        await ragSystem.AskLimsRAG("How do I fix my broken centrifuge?"); // Query for which we have no direct info
        Console.WriteLine("\n---------------------------------");
    }
}