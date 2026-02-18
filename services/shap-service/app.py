"""
SHAP Service for Digital Twin Platform
Provides SHAP value calculation for ML.NET models via ONNX
"""
from flask import Flask, request, jsonify
from flask_cors import CORS
import numpy as np
import shap
import onnxruntime as ort
import logging
import os

app = Flask(__name__)
CORS(app)

logging.basicConfig(level=logging.INFO)
logger = logging.getLogger(__name__)

# Global model cache
_model_cache = {}
_background_data_cache = {}


def load_onnx_model(model_path: str):
    """Load ONNX model from file path"""
    if model_path in _model_cache:
        return _model_cache[model_path]
    
    if not os.path.exists(model_path):
        raise FileNotFoundError(f"Model file not found: {model_path}")
    
    session = ort.InferenceSession(model_path)
    _model_cache[model_path] = session
    logger.info(f"Loaded ONNX model: {model_path}")
    return session


def predict_with_onnx(session, features: np.ndarray):
    """Run prediction using ONNX model"""
    input_name = session.get_inputs()[0].name
    output = session.run(None, {input_name: features.astype(np.float32)})
    return output[0]


@app.route('/health', methods=['GET'])
def health():
    """Health check endpoint"""
    return jsonify({"status": "healthy", "service": "shap-service"})


@app.route('/calculate-shap', methods=['POST'])
def calculate_shap():
    """
    Calculate SHAP values for given features
    
    Request body:
    {
        "model_path": "path/to/model.onnx",
        "features": [1.0, 2.0, ...],
        "feature_names": ["feature1", "feature2", ...],
        "background_data": [[1.0, 2.0, ...], ...]
    }
    """
    try:
        data = request.json
        
        model_path = data.get('model_path')
        features = np.array(data['features']).reshape(1, -1)
        feature_names = data.get('feature_names', [f'feature_{i}' for i in range(len(features[0]))])
        background_data = np.array(data.get('background_data', []))
        
        if model_path is None:
            return jsonify({"error": "model_path is required"}), 400
        
        # Load model
        session = load_onnx_model(model_path)
        
        # Prepare background data (use provided or generate from cache)
        if len(background_data) == 0:
            # Use cached background data or generate default
            cache_key = f"{model_path}_background"
            if cache_key in _background_data_cache:
                background_data = _background_data_cache[cache_key]
            else:
                # Generate simple background data (mean-centered)
                background_data = np.random.normal(
                    features.mean(), 
                    features.std(), 
                    (100, features.shape[1])
                )
                _background_data_cache[cache_key] = background_data
        
        # Create SHAP explainer
        # For tree models, use TreeExplainer; for others, use KernelExplainer
        try:
            # Try TreeExplainer first (faster, more accurate for tree models)
            explainer = shap.TreeExplainer(lambda x: predict_with_onnx(session, x))
            shap_values = explainer.shap_values(features, background_data)
        except Exception as e:
            logger.warning(f"TreeExplainer failed, using KernelExplainer: {e}")
            # Fallback to KernelExplainer (works for any model, but slower)
            explainer = shap.KernelExplainer(
                lambda x: predict_with_onnx(session, x),
                background_data
            )
            shap_values = explainer.shap_values(features)
        
        # Handle multi-output models
        if isinstance(shap_values, list):
            shap_values = shap_values[0]
        
        # Convert to feature contributions dictionary
        contributions = {}
        if len(shap_values.shape) > 1:
            shap_values = shap_values[0]  # Take first row if 2D
        
        for i, name in enumerate(feature_names):
            if i < len(shap_values):
                contributions[name] = float(shap_values[i])
        
        base_value = float(explainer.expected_value) if hasattr(explainer, 'expected_value') else 0.0
        
        return jsonify({
            'contributions': contributions,
            'base_value': base_value,
            'shap_values': shap_values.tolist() if hasattr(shap_values, 'tolist') else []
        })
    
    except Exception as e:
        logger.error(f"Error calculating SHAP values: {e}", exc_info=True)
        return jsonify({"error": str(e)}), 500


@app.route('/update-background', methods=['POST'])
def update_background():
    """Update background dataset for a model"""
    try:
        data = request.json
        model_path = data.get('model_path')
        background_data = np.array(data.get('background_data', []))
        
        if model_path is None:
            return jsonify({"error": "model_path is required"}), 400
        
        cache_key = f"{model_path}_background"
        _background_data_cache[cache_key] = background_data
        
        return jsonify({
            "status": "success",
            "samples": len(background_data)
        })
    
    except Exception as e:
        logger.error(f"Error updating background data: {e}", exc_info=True)
        return jsonify({"error": str(e)}), 500


if __name__ == '__main__':
    port = int(os.environ.get('PORT', 5000))
    app.run(host='0.0.0.0', port=port, debug=False)
