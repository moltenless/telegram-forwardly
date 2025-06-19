from functools import wraps

from flask import Blueprint, request, jsonify, current_app

from app import Config
from app.async_loop_manager import event_loop_manager
from app.utils import parse_user_from_api, logger

api_bp = Blueprint('api', __name__)


def require_api_key(f):
    @wraps(f)
    def decorated(*args, **kwargs):
        api_key = request.headers.get('X-Api-Key')
        expected_api_key = Config.API_KEY
        if not api_key or api_key != expected_api_key:
            logger.error(f"Invalid or missing API key: {api_key}")
            return jsonify({'Success': False, 'ErrorMessage': 'Invalid or missing API key'}), 401
        return f(*args, **kwargs)
    return decorated


@api_bp.route('/user/launch', methods=['POST'])
@require_api_key
def launch_user():
    try:
        data = request.get_json()
        user = parse_user_from_api(data)

        result = event_loop_manager.run_coroutine(
            current_app.client_manager.launch_client(user)
        )

        if not result:
            logger.error(f'Client is not authorized! Silent error.')
            return jsonify({'Success': False, 'ErrorMessage': 'Authentication failed.'}), 500

        return jsonify({'Success': True}), 200

    except Exception as e:
        logger.error(f'Failed to launch new user. {e}')
        return jsonify({'Success': False, 'ErrorMessage': 'Authentication failed.'}), 500

@api_bp.route('/user/forum', methods=['POST'])
@require_api_key
def check_and_update_forum():
    try:
        data = request.get_json()
        user_id = data.get('user_id')
        forum_id = data.get('forum_id')

        result = event_loop_manager.run_coroutine(
            current_app.client_manager.check_and_update_forum(user_id, forum_id)
        )

        if result.get('Success') is True:
            return jsonify(result), 200
        else:
            return jsonify(result), 500

    except Exception as e:
        return jsonify({"Success": False,
                        "ErrorMessage": f"Failed to check and update forum id: {e}"}), 500

@api_bp.route('/user/grouping', methods=['POST'])
@require_api_key
def update_grouping():
    try:
        data = request.get_json()
        user_id = data.get('user_id')
        grouping = data.get('grouping')

        result = event_loop_manager.run_coroutine(
            current_app.client_manager.update_grouping(user_id, grouping)
        )

        if result.get('Success') is True:
            return jsonify(result), 200
        else:
            return jsonify(result), 500

    except Exception as e:
        return jsonify({"Success": False,
                        "ErrorMessage": f"Failed to update grouping type: {e}"}), 500

@api_bp.route('/user/delete', methods=['POST'])
@require_api_key
def delete_user():
    try:
        data = request.get_json()
        user_id = data.get('user_id')

        result = event_loop_manager.run_coroutine(
            current_app.client_manager.delete_user(user_id)
        )

        if result.get('Success') is True:
            return jsonify(result), 200
        else:
            return jsonify(result), 500

    except Exception as e:
        return jsonify({"Success": False,
                        "ErrorMessage": f"Failed to delete user data type: {e}"}), 500


@api_bp.route('/user/chats/all', methods=['POST'])
@require_api_key
def set_all_chats_enabled():
    try:
        data = request.get_json()
        user_id = data.get('user_id')
        enable_all_chats = data.get('enable_all_chats')

        result = event_loop_manager.run_coroutine(
            current_app.client_manager.enable_all_chats(user_id, enable_all_chats)
        )

        if result.get('Success') is True:
            return jsonify(result), 200
        else:
            return jsonify(result), 500

    except Exception as e:
        return jsonify({"Success": False,
                        "ErrorMessage": f"Failed to set all chats to listen: {e}"}), 500


@api_bp.route('/user/<int:user_id>/chats', methods=['GET'])
@require_api_key
def get_user_chats(user_id):
    try:
        result = event_loop_manager.run_coroutine(
            current_app.client_manager.get_user_chats(user_id)
        )

        if result.get('Success') is True:
            return jsonify(result), 200
        else:
            return jsonify(result), 500

    except Exception as e:
        return jsonify({
            'Success': False,
            'ErrorMessage': f'Failed to retrieve chats: {e}'
        }), 500


@api_bp.route('/user/chats/add', methods=['POST'])
@require_api_key
def add_chats():
    try:
        data = request.get_json()
        user_id = data.get('user_id')
        chats = data.get('chats')

        result = event_loop_manager.run_coroutine(
            current_app.client_manager.add_chats(user_id, chats)
        )

        if result.get('Success') is True:
            return jsonify(result), 200
        else:
            return jsonify(result), 500

    except Exception as e:
        return jsonify({
            'Success': False,
            'ErrorMessage': f'Error adding chats to user: {e}'
        }), 500

@api_bp.route('/user/chats/remove', methods=['POST'])
@require_api_key
def remove_chats():
    try:
        data = request.get_json()
        user_id = data.get('user_id')
        chats = data.get('chats')

        result = event_loop_manager.run_coroutine(
            current_app.client_manager.remove_chats(user_id, chats)
        )

        if result.get('Success') is True:
            return jsonify(result), 200
        else:
            return jsonify(result), 500

    except Exception as e:
        return jsonify({
            'Success': False,
            'ErrorMessage': f'Error removing chats for user: {e}'
        }), 500


@api_bp.route('/user/keywords/add', methods=['POST'])
@require_api_key
def add_keywords():
    try:
        data = request.get_json()
        user_id = data.get('user_id')
        keywords = data.get('keywords')

        result = event_loop_manager.run_coroutine(
            current_app.client_manager.add_keywords(user_id, keywords)
        )

        if result.get('Success') is True:
            return jsonify(result), 200
        else:
            return jsonify(result), 500

    except Exception as e:
        return jsonify({
            'Success': False,
            'ErrorMessage': f'Error adding keywords to user: {e}'
        }), 500

@api_bp.route('/user/keywords/remove', methods=['POST'])
@require_api_key
def remove_keywords():
    try:
        data = request.get_json()
        user_id = data.get('user_id')
        keywords_without_special_characters = data.get('keywordsWithoutSpecialCharacters')

        result = event_loop_manager.run_coroutine(
            current_app.client_manager.remove_keywords(
                user_id,
                keywords_without_special_characters)
        )

        if result.get('Success') is True:
            return jsonify(result), 200
        else:
            return jsonify(result), 500

    except Exception as e:
        return jsonify({
            'Success': False,
            'ErrorMessage': f'Error removing keywords for user: {e}'
        }), 500

@api_bp.route('/user/forwardly', methods=['POST'])
@require_api_key
def toggle_forwarding():
    try:
        data = request.get_json()
        user_id = data.get('user_id')
        value = data.get('value')

        result = event_loop_manager.run_coroutine(
            current_app.client_manager.toggle_forwarding(user_id, value)
        )

        if result.get('Success') is True:
            return jsonify(result), 200
        else:
            return jsonify(result), 500

    except Exception as e:
        return jsonify({"Success": False,
                        "ErrorMessage": f"Failed to toggle forwarding: {e}"}), 500



