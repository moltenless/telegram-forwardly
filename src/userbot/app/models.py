from dataclasses import dataclass, field
from typing import List, Optional
from enum import Enum
from telethon import TelegramClient

class UserState(Enum):
    IDLE = "Idle"
    AWAITING_PHONE_NUMBER = "AwaitingPhoneNumber"
    AWAITING_API_ID = "AwaitingApiId"
    AWAITING_API_HASH = "AwaitingApiHash"
    AWAITING_SESSION_STRING = "AwaitingSessionString"
    AWAITING_ENABLE_ALL_CHATS = "AwaitingEnableAllChats"
    AWAITING_CHATS = "AwaitingChats"
    AWAITING_KEYWORDS = "AwaitingKeywords"
    AWAITING_FORUM_GROUP = "AwaitingForumGroup"
    AWAITING_GROUPING_TYPE = "AwaitingGroupingType"
    AWAITING_DELETE_CONFIRMATION = "AwaitingDeleteConfirmation"
    AWAITING_REMOVE_CHATS = "AwaitingRemoveChats"

class GroupingMode(Enum):
    BY_KEYWORD = "ByKeyword"
    BY_CHAT = "ByChat"

@dataclass
class Keyword:
    id: int
    telegram_user_id: int
    value: str

@dataclass
class Chat:
    id: int
    telegram_user_id: int
    telegram_chat_id: int
    title: str

@dataclass
class BotUser:
    telegram_user_id: int
    current_state: Optional[UserState] = None
    api_id: Optional[str] = None
    api_hash: Optional[str] = None
    session_string: Optional[str] = None
    phone: Optional[str] = None
    is_authenticated: Optional[bool] = None
    registration_date_time: Optional[str] = None
    user_name: Optional[str] = None
    first_name: Optional[str] = None
    forum_supergroup_id: Optional[int] = None
    topic_grouping: Optional[GroupingMode] = None
    forwardly_enabled: Optional[bool] = None
    all_chats_filtering_enabled: Optional[bool] = None

    keywords: List[Keyword] = field(default_factory=list)
    chats: List[Chat] = field(default_factory=list)
    
class UserClient:
    user: BotUser
    client: Optional[TelegramClient] = None
    is_connected: bool = False
    last_error: Optional[str] = None
