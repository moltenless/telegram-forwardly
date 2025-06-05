from flask import Blueprint, request, jsonify, current_app
import asyncio
from app.utils import log_error, parse_user_from_api
from app.async_loop_manager import event_loop_manager

api_bp = Blueprint('api', __name__)

@api_bp.route('/user/update', methods=['POST'])
def update_user():
    try:
        data = request.get_json()
        user_data = parse_user_from_api(data)

        result = event_loop_manager.run_coroutine(
            current_app.client_manager.update_user(user_data)
        )

        return jsonify(result)

    except Exception as e:
        log_error("Error updating user", e, {'telegram_user_id': request.get_json().get('telegram_user_id')})
        return jsonify({'error': str(e)}), 500


@api_bp.route('/user/remove', methods=['POST'])
def remove_user():
    try:
        data = request.get_json()
        telegram_user_id = data.get('telegram_user_id')

        if not telegram_user_id:
            return jsonify({'error': 'Missing telegram_user_id'}), 400

        result = event_loop_manager.run_coroutine(
            current_app.client_manager.remove_user(telegram_user_id)
        )

        return jsonify(result)

    except Exception as e:
        log_error("Error removing user", e,
                  {'telegram_user_id': request.get_json().get('telegram_user_id')})
        return jsonify({'error': str(e)}), 500

@api_bp.route('/users/status', methods=['GET'])
def get_users_status():
    try:
        status = current_app.client_manager.get_all_users_status()
        return jsonify(status)
    except Exception as e:
        log_error("Error getting users status", e)
        return jsonify({'error': str(e)}), 500