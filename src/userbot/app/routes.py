from flask import Blueprint, request, jsonify, current_app
import asyncio
from app.utils import log_info, log_error, parse_user_from_api
from app.models import BotUser

api_bp = Blueprint('api', __name__)

@api_bp.route('/health', methods=['GET'])
def health_check():
    return jsonify({
        'status': 'healthy',
        'service': 'userbot',
        'connected_clients': len(current_app.client_manager.clients)
    })

@api_bp.route('/auth/start', methods=['POST'])
def start_auth():
    try:
        data = request.get_json()
        telegram_user_id = data.get('telegram_user_id')
        phone = data.get('phone')
        api_id = data.get('api_id')
        api_hash = data.get('api_hash')

        if not all([telegram_user_id, phone, api_id, api_hash]):
            return jsonify({'error': 'Missing required fields'}), 400

        loop = asyncio.new_event_loop()
        asyncio.set_event_loop(loop)

        result = loop.run_until_complete(
            current_app.client_manager.start_authentication(
                telegram_user_id, phone, api_id, api_hash
            )
        )

        return jsonify(result)

    except Exception as e:
        log_error(
            "Error starting authentication", e,
            {'telegram_user_id': data.get('telegram_user_id')})
        return jsonify({'error': str(e)}), 500

@api_bp.route('/auth/verify', methods=['POST'])
def verify_code():
    try:
        data = request.get_json()
        telegram_user_id = data.get('telegram_user_id')
        code = data.get('code')

        if not all([telegram_user_id, code]):
            return jsonify({'error': 'Missinng required fields'}), 400

        loop = asyncio.new_event_loop()
        asyncio.set_event_loop(loop)

        result = loop.run_until_complete(
            current_app.client_manager.verify_code(telegram_user_id, code)
        )

        return jsonify(result)

    except Exception as e:
        log_error("Error verifying code", e,
                  {'user_id': data.get('user_id')})
        return jsonify({'error': str(e)}), 500


@api_bp.route('/auth/password', methods=['POST'])
def verify_password():
    try:
        data = request.get_json()
        telegram_user_id = data.get('telegram_user_id')
        password = data.get('password')

        if not all([telegram_user_id, password]):
            return jsonify({'error': 'Missing required fields'}), 400

        loop = asyncio.new_event_loop()
        asyncio.set_event_loop(loop)

        result = loop.run_until_complete(
            current_app.client_manager.verify_password(telegram_user_id, password)
        )

        return jsonify(result)

    except Exception as e:
        log_error("Error verifying password", e,
                  {'user_id': data.get('user_id')})
        return jsonify({'error': str(e)}), 500


@api_bp.route('/user/update', methods=['POST'])
def update_user():
    try:
        data = request.get_json()
        user_data = parse_user_from_api(data)

        loop = asyncio.new_event_loop()
        asyncio.set_event_loop(loop)

        result = loop.run_until_complete(
            current_app.client_manager.update_user(user_data)
        )

        return jsonify(result)

    except Exception as e:
        log_error("Error updating user", e, {'telegram_user_id': data.get('telegram_user_id')})
        return jsonify({'error': str(e)}), 500


@api_bp.route('/user/remove', methods=['POST'])
def remove_user():
    try:
        data = request.get_json()
        telegram_user_id = data.get('telegram_user_id')

        if not telegram_user_id:
            return jsonify({'error': 'Missing telegram_user_id'}), 400

        loop = asyncio.new_event_loop()
        asyncio.set_event_loop(loop)

        result = loop.run_until_complete(
            current_app.client_manager.remove_user(telegram_user_id)
        )

        return jsonify(result)

    except Exception as e:
        log_error("Error removing user", e,
                  {'telegram_user_id': data.get('telegram_user_id')})
        return jsonify({'error': str(e)}), 500

@api_bp.route('/users/status', methods=['GET'])
def get_users_status():
    try:
        status = current_app.client_manager.get_all_users_status()
        return jsonify(status)
    except Exception as e:
        log_error("Error getting users status", e)
        return jsonify({'error': str(e)}), 500