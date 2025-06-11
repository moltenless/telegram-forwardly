import asyncio
import threading

class EventLoopManager:
    def __init__(self):
        self.loop = None
        self._thread = None

    def start_loop(self):
        self.loop = asyncio.new_event_loop()
        self._thread = threading.Thread(target=self._run_loop, daemon=True)
        self._thread.start()

    def _run_loop(self):
        asyncio.set_event_loop(self.loop)
        self.loop.run_forever()

    def run_coroutine(self, coro):
        future = asyncio.run_coroutine_threadsafe(coro, self.loop)
        return future.result()

event_loop_manager = EventLoopManager()