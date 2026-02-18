using Microsoft.AspNetCore.Mvc;
using DigitalTwinPlatform.Application.ML.Models;
using DigitalTwinPlatform.Application.ML.Services;
using DigitalTwinPlatform.Application.ML.Queries;
using DigitalTwinPlatform.Application.ML.Commands;
using MediatR;
using System.ComponentModel.DataAnnotations;

namespace DigitalTwinPlatform.API.Controllers
{
    /// <summary>
    /// Controller for AI/ML Model Management
    /// Handles deployment, management, and monitoring of predictive models
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class AIModelController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly IAIService _aiService;
        private readonly ILogger<AIModelController> _logger;

        public AIModelController(
            IMediator mediator,
            IAIService aiService,
            ILogger<AIModelController> logger)
        {
            _mediator = mediator;
            _aiService = aiService;
            _logger = logger;
        }

        /// <summary>
        /// Get all deployed AI models
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<AIModelDto>), 200)]
        public async Task<ActionResult<IEnumerable<AIModelDto>>> GetAllModels()
        {
            try
            {
                var query = new GetAllAIModelsQuery();
                var models = await _mediator.Send(query);
                return Ok(models);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving AI models");
                return StatusCode(500, new { Message = "Internal server error" });
            }
        }

        /// <summary>
        /// Get specific AI model by ID
        /// </summary>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(AIModelDto), 200)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<AIModelDto>> GetModel(string id)
        {
            try
            {
                var query = new GetAIModelByIdQuery(id);
                var model = await _mediator.Send(query);
                
                if (model == null)
                    return NotFound(new { Message = $"Model with ID {id} not found" });
                    
                return Ok(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving AI model {ModelId}", id);
                return StatusCode(500, new { Message = "Internal server error" });
            }
        }

        /// <summary>
        /// Deploy a new AI model
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(AIModelDto), 201)]
        [ProducesResponseType(400)]
        public async Task<ActionResult<AIModelDto>> DeployModel([FromBody] DeployAIModelCommand command)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var model = await _mediator.Send(command);
                return CreatedAtAction(nameof(GetModel), new { id = model.Id }, model);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deploying AI model");
                return StatusCode(500, new { Message = "Internal server error" });
            }
        }

        /// <summary>
        /// Update existing AI model
        /// </summary>
        [HttpPut("{id}")]
        [ProducesResponseType(typeof(AIModelDto), 200)]
        [ProducesResponseType(404)]
        [ProducesResponseType(400)]
        public async Task<ActionResult<AIModelDto>> UpdateModel(string id, [FromBody] UpdateAIModelCommand command)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                command.Id = id; // Ensure ID consistency
                var model = await _mediator.Send(command);
                
                if (model == null)
                    return NotFound(new { Message = $"Model with ID {id} not found" });
                    
                return Ok(model);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating AI model {ModelId}", id);
                return StatusCode(500, new { Message = "Internal server error" });
            }
        }

        /// <summary>
        /// Delete AI model
        /// </summary>
        [HttpDelete("{id}")]
        [ProducesResponseType(204)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> DeleteModel(string id)
        {
            try
            {
                var command = new DeleteAIModelCommand(id);
                await _mediator.Send(command);
                return NoContent();
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting AI model {ModelId}", id);
                return StatusCode(500, new { Message = "Internal server error" });
            }
        }

        /// <summary>
        /// Test model prediction
        /// </summary>
        [HttpPost("{id}/predict")]
        [ProducesResponseType(typeof(ModelPredictionDto), 200)]
        [ProducesResponseType(404)]
        [ProducesResponseType(400)]
        public async Task<ActionResult<ModelPredictionDto>> Predict(string id, [FromBody] ModelInputDto input)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var prediction = await _aiService.Predict(id, input.Features);
                return Ok(prediction);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
            catch (KeyNotFoundException)
            {
                return NotFound(new { Message = $"Model with ID {id} not found" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error making prediction with model {ModelId}", id);
                return StatusCode(500, new { Message = "Internal server error" });
            }
        }

        /// <summary>
        /// Get model performance metrics
        /// </summary>
        [HttpGet("{id}/metrics")]
        [ProducesResponseType(typeof(ModelMetricsDto), 200)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<ModelMetricsDto>> GetModelMetrics(string id)
        {
            try
            {
                var metrics = await _aiService.GetModelMetrics(id);
                return Ok(metrics);
            }
            catch (KeyNotFoundException)
            {
                return NotFound(new { Message = $"Model with ID {id} not found" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving model metrics for {ModelId}", id);
                return StatusCode(500, new { Message = "Internal server error" });
            }
        }

        /// <summary>
        /// Retrain model with new data
        /// </summary>
        [HttpPost("{id}/retrain")]
        [ProducesResponseType(typeof(AIModelDto), 200)]
        [ProducesResponseType(404)]
        [ProducesResponseType(400)]
        public async Task<ActionResult<AIModelDto>> RetrainModel(string id, [FromBody] RetrainModelCommand command)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                command.ModelId = id;
                var model = await _mediator.Send(command);
                
                if (model == null)
                    return NotFound(new { Message = $"Model with ID {id} not found" });
                    
                return Ok(model);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retraining model {ModelId}", id);
                return StatusCode(500, new { Message = "Internal server error" });
            }
        }

        /// <summary>
        /// Get compatible machines for model deployment
        /// </summary>
        [HttpGet("{id}/compatible-machines")]
        [ProducesResponseType(typeof(IEnumerable<string>), 200)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<IEnumerable<string>>> GetCompatibleMachines(string id)
        {
            try
            {
                var machines = await _aiService.GetCompatibleMachines(id);
                return Ok(machines);
            }
            catch (KeyNotFoundException)
            {
                return NotFound(new { Message = $"Model with ID {id} not found" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving compatible machines for model {ModelId}", id);
                return StatusCode(500, new { Message = "Internal server error" });
            }
        }

        /// <summary>
        /// Deploy model to specific machines
        /// </summary>
        [HttpPost("{id}/deploy-to-machines")]
        [ProducesResponseType(200)]
        [ProducesResponseType(404)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> DeployToMachines(string id, [FromBody] DeployToMachinesDto request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                await _aiService.DeployModelToMachines(id, request.MachineIds);
                return Ok(new { Message = "Model deployed to specified machines successfully" });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
            catch (KeyNotFoundException)
            {
                return NotFound(new { Message = $"Model with ID {id} not found" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deploying model {ModelId} to machines", id);
                return StatusCode(500, new { Message = "Internal server error" });
            }
        }

        /// <summary>
        /// Get model deployment status
        /// </summary>
        [HttpGet("{id}/deployment-status")]
        [ProducesResponseType(typeof(DeploymentStatusDto), 200)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<DeploymentStatusDto>> GetDeploymentStatus(string id)
        {
            try
            {
                var status = await _aiService.GetDeploymentStatus(id);
                return Ok(status);
            }
            catch (KeyNotFoundException)
            {
                return NotFound(new { Message = $"Model with ID {id} not found" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving deployment status for model {ModelId}", id);
                return StatusCode(500, new { Message = "Internal server error" });
            }
        }

        /// <summary>
        /// Validate model file before deployment
        /// </summary>
        [HttpPost("validate-model")]
        [ProducesResponseType(typeof(ModelValidationResultDto), 200)]
        [ProducesResponseType(400)]
        public async Task<ActionResult<ModelValidationResultDto>> ValidateModel([FromBody] ValidateModelDto request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var validationResult = await _aiService.ValidateModel(request.ModelFile, request.ModelType);
                return Ok(validationResult);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating model file");
                return StatusCode(500, new { Message = "Internal server error" });
            }
        }
    }

    // DTOs for the controller
    public class DeployToMachinesDto
    {
        [Required]
        public IEnumerable<string> MachineIds { get; set; } = new List<string>();
    }

    public class ValidateModelDto
    {
        [Required]
        public byte[] ModelFile { get; set; } = [];
        
        [Required]
        public string ModelType { get; set; } = string.Empty;
    }
}