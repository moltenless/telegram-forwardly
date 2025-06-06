import datetime
from datetime import timezone
import logging
import json
from dataclasses import asdict, is_dataclass
from enum import Enum
from typing import Dict, Any, Optional
from datetime import datetime
from app.models import BotUser, Keyword, Chat, GroupingMode, UserState

logger = logging.getLogger(__name__)

def log_error(message: str, error: Exception = None, extra_data: Dict[str, Any] = None):
    log_data = {
        'timestamp': datetime.now(timezone.utc).isoformat(),
        'message': message,
        'extra_data': convert_to_serializable(extra_data) if extra_data else {}
    }
    if error:
        log_data['error'] = str(error)
        log_data['error_type'] = type(error).__name__
    logger.error(json.dumps(log_data))

def log_info(message: str, extra_data: Dict[str, Any] = None):
    log_data = {
        'timestamp': datetime.now(timezone.utc).isoformat(),
        'message': message,
        'extra_data': convert_to_serializable(extra_data) if extra_data else {}
    }
    logger.info(json.dumps(log_data))

def safe_int_convert(value: Any) -> Optional[int]:
    try:
        return int(value) if value is not None else None
    except (ValueError, TypeError):
        return None

def parse_user_from_api(data: Dict[str, Any]) -> BotUser:
    keywords = [
        Keyword(
            id=kw.get('Id'),
            telegram_user_id=kw.get('TelegramUserId'),
            value=kw.get('Value')
        )
        for kw in data.get('Keywords', [])
    ]

    chats = [
        Chat(
            id=chat.get('Id'),
            telegram_user_id=chat.get('TelegramUserId'),
            telegram_chat_id=chat.get('TelegramChatId')
        )
        for chat in data.get('Chats', [])
    ]

    return BotUser(
        telegram_user_id=data.get('TelegramUserId'),
        current_state=UserState(data.get('CurrentState')) if data.get('CurrentState') else None,
        api_id=data.get('ApiId'),
        api_hash=data.get('ApiHash'),
        session_string=data.get('SessionString'),
        phone=data.get('Phone'),
        is_authenticated=data.get('IsAuthenticated'),
        registration_date_time=data.get('RegistrationDateTime'),
        user_name=data.get('UserName'),
        first_name=data.get('FirstName'),
        forum_supergroup_id=safe_int_convert(data.get('ForumSupergroupId')),
        topic_grouping=GroupingMode(data.get('TopicGrouping')) if data.get('TopicGrouping') else None,
        forwardly_enabled=data.get('ForwardlyEnabled'),
        all_chats_filtering_enabled=data.get('AllChatsFilteringEnabled'),
        keywords=keywords,
        chats=chats
    )

def convert_to_serializable(obj: Any) -> Any:
    if is_dataclass(obj):
        return {k: convert_to_serializable(v) for k, v in asdict(obj).items()}
    elif isinstance(obj, Enum):
        return obj.value
    elif isinstance(obj, (list, tuple)):
        return [convert_to_serializable(item) for item in obj]
    elif isinstance(obj, dict):
        return {k: convert_to_serializable(v) for k, v in obj.items()}
    elif hasattr(obj, '__dict__'):
        return convert_to_serializable(vars(obj))
    return obj