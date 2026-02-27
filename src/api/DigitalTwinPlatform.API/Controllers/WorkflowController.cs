using Microsoft.AspNetCore.Authorization;
using DigitalTwinPlatform.Application.Workflows.Models;
using Microsoft.AspNetCore.Mvc;
using DigitalTwinPlatform.Application.Workflows.Services;
using DigitalTwinPlatform.Application.Workflows.Queries;
using DigitalTwinPlatform.Application.Workflows.Commands;
using MediatR;
using System.ComponentModel.DataAnnotations;

namespace DigitalTwinPlatform.API.Controllers
{
    /// <summary>
    /// Controller for Workflow Automation
    /// Handles creation, management, and execution of automated workflows
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class WorkflowController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly IWorkflowService _workflowService;
        private readonly ILogger<WorkflowController> _logger;

        public WorkflowController(
            IMediator mediator,
            IWorkflowService workflowService,
            ILogger<WorkflowController> logger)
        {
            _mediator = mediator;
            _workflowService = workflowService;
            _logger = logger;
        }

        /// <summary>
        /// Get all workflows
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<WorkflowDefinitionDto>), 200)]
        public async Task<ActionResult<IEnumerable<WorkflowDefinitionDto>>> GetAllWorkflows()
        {
            try
            {
                var query = new GetAllWorkflowsQuery();
                var workflows = await _mediator.Send(query);
                return Ok(workflows);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving workflows");
                return StatusCode(500, new { Message = "Internal server error" });
            }
        }

        /// <summary>
        /// Get specific workflow by ID
        /// </summary>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(WorkflowDefinitionDto), 200)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<WorkflowDefinitionDto>> GetWorkflow(string id)
        {
            try
            {
                var query = new GetWorkflowByIdQuery(id);
                var workflow = await _mediator.Send(query);
                
                if (workflow == null)
                    return NotFound(new { Message = $"Workflow with ID {id} not found" });
                    
                return Ok(workflow);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving workflow {WorkflowId}", id);
                return StatusCode(500, new { Message = "Internal server error" });
            }
        }

        /// <summary>
        /// Create a new workflow
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(WorkflowDefinitionDto), 201)]
        [ProducesResponseType(400)]
        public async Task<ActionResult<WorkflowDefinitionDto>> CreateWorkflow([FromBody] CreateWorkflowCommand command)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var workflow = await _mediator.Send(command);
                return CreatedAtAction(nameof(GetWorkflow), new { id = workflow.Id }, workflow);
            }
            catch (ArgumentException)
            {
                return BadRequest(new { Message = "Invalid argument provided." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating workflow");
                return StatusCode(500, new { Message = "Internal server error" });
            }
        }

        /// <summary>
        /// Update existing workflow
        /// </summary>
        [HttpPut("{id}")]
        [ProducesResponseType(typeof(WorkflowDefinitionDto), 200)]
        [ProducesResponseType(404)]
        [ProducesResponseType(400)]
        public async Task<ActionResult<WorkflowDefinitionDto>> UpdateWorkflow(string id, [FromBody] UpdateWorkflowCommand command)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                command.Id = id;
                var workflow = await _mediator.Send(command);
                
                if (workflow == null)
                    return NotFound(new { Message = $"Workflow with ID {id} not found" });
                    
                return Ok(workflow);
            }
            catch (ArgumentException)
            {
                return BadRequest(new { Message = "Invalid argument provided." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating workflow {WorkflowId}", id);
                return StatusCode(500, new { Message = "Internal server error" });
            }
        }

        /// <summary>
        /// Delete workflow
        /// </summary>
        [HttpDelete("{id}")]
        [ProducesResponseType(204)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> DeleteWorkflow(string id)
        {
            try
            {
                var command = new DeleteWorkflowCommand(id);
                await _mediator.Send(command);
                return NoContent();
            }
            catch (ArgumentException)
            {
                return BadRequest(new { Message = "Invalid argument provided." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting workflow {WorkflowId}", id);
                return StatusCode(500, new { Message = "Internal server error" });
            }
        }

        /// <summary>
        /// Enable/disable workflow
        /// </summary>
        [HttpPost("{id}/toggle")]
        [ProducesResponseType(typeof(WorkflowDefinitionDto), 200)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<WorkflowDefinitionDto>> ToggleWorkflow(string id)
        {
            try
            {
                var command = new ToggleWorkflowCommand(id);
                var workflow = await _mediator.Send(command);
                
                if (workflow == null)
                    return NotFound(new { Message = $"Workflow with ID {id} not found" });
                
                var action = workflow.Enabled ? "enabled" : "disabled";
                return Ok(new { Message = $"Workflow {action} successfully", Workflow = workflow });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error toggling workflow {WorkflowId}", id);
                return StatusCode(500, new { Message = "Internal server error" });
            }
        }

        /// <summary>
        /// Execute workflow manually
        /// </summary>
        [HttpPost("{id}/execute")]
        [ProducesResponseType(typeof(WorkflowExecutionDto), 200)]
        [ProducesResponseType(404)]
        [ProducesResponseType(400)]
        public async Task<ActionResult<WorkflowExecutionDto>> ExecuteWorkflow(string id, [FromBody] ExecuteWorkflowDto request)
        {
            try
            {
                var execution = await _workflowService.ExecuteWorkflow(id, request.Context);
                return Ok(execution);
            }
            catch (ArgumentException)
            {
                return BadRequest(new { Message = "Invalid argument provided." });
            }
            catch (InvalidOperationException)
            {
                return BadRequest(new { Message = "Operation is not valid for the current state." });
            }
            catch (KeyNotFoundException)
            {
                return NotFound(new { Message = $"Workflow with ID {id} not found" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error executing workflow {WorkflowId}", id);
                return StatusCode(500, new { Message = "Internal server error" });
            }
        }

        /// <summary>
        /// Get workflow execution history
        /// </summary>
        [HttpGet("{id}/executions")]
        [ProducesResponseType(typeof(IEnumerable<WorkflowExecutionDto>), 200)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<IEnumerable<WorkflowExecutionDto>>> GetWorkflowExecutions(
            string id,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20)
        {
            try
            {
                var executions = await _workflowService.GetWorkflowExecutions(id, page, pageSize);
                return Ok(executions);
            }
            catch (KeyNotFoundException)
            {
                return NotFound(new { Message = $"Workflow with ID {id} not found" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving workflow executions for {WorkflowId}", id);
                return StatusCode(500, new { Message = "Internal server error" });
            }
        }

        /// <summary>
        /// Get workflow statistics
        /// </summary>
        [HttpGet("{id}/statistics")]
        [ProducesResponseType(typeof(WorkflowStatisticsDto), 200)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<WorkflowStatisticsDto>> GetWorkflowStatistics(string id)
        {
            try
            {
                var stats = await _workflowService.GetWorkflowStatistics(id);
                return Ok(stats);
            }
            catch (KeyNotFoundException)
            {
                return NotFound(new { Message = $"Workflow with ID {id} not found" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving workflow statistics for {WorkflowId}", id);
                return StatusCode(500, new { Message = "Internal server error" });
            }
        }

        /// <summary>
        /// Validate workflow definition
        /// </summary>
        [HttpPost("validate")]
        [ProducesResponseType(typeof(WorkflowValidationResultDto), 200)]
        [ProducesResponseType(400)]
        public async Task<ActionResult<WorkflowValidationResultDto>> ValidateWorkflow([FromBody] ValidateWorkflowDto request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var validationResult = await _workflowService.ValidateWorkflow(request.Definition);
                return Ok(validationResult);
            }
            catch (ArgumentException)
            {
                return BadRequest(new { Message = "Invalid argument provided." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating workflow");
                return StatusCode(500, new { Message = "Internal server error" });
            }
        }

        /// <summary>
        /// Get workflow templates
        /// </summary>
        [HttpGet("templates")]
        [ProducesResponseType(typeof(IEnumerable<WorkflowTemplateDto>), 200)]
        public async Task<ActionResult<IEnumerable<WorkflowTemplateDto>>> GetWorkflowTemplates()
        {
            try
            {
                var templates = await _workflowService.GetWorkflowTemplates();
                return Ok(templates);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving workflow templates");
                return StatusCode(500, new { Message = "Internal server error" });
            }
        }

        /// <summary>
        /// Duplicate workflow
        /// </summary>
        [HttpPost("{id}/duplicate")]
        [ProducesResponseType(typeof(WorkflowDefinitionDto), 201)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<WorkflowDefinitionDto>> DuplicateWorkflow(string id, [FromBody] DuplicateWorkflowDto request)
        {
            try
            {
                var command = new DuplicateWorkflowCommand
                {
                    SourceWorkflowId = id,
                    NewName = request.NewName,
                    NewDescription = request.NewDescription
                };
                
                var workflow = await _mediator.Send(command);
                return CreatedAtAction(nameof(GetWorkflow), new { id = workflow.Id }, workflow);
            }
            catch (ArgumentException)
            {
                return BadRequest(new { Message = "Invalid argument provided." });
            }
            catch (KeyNotFoundException)
            {
                return NotFound(new { Message = $"Workflow with ID {id} not found" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error duplicating workflow {WorkflowId}", id);
                return StatusCode(500, new { Message = "Internal server error" });
            }
        }
    }
}