# https://stackoverflow.com/a/1146404/9731532
from os import system, environ
import win32con
from win32gui import SendMessage
from winreg import (
    CloseKey, OpenKey, QueryValueEx, SetValueEx,
    HKEY_CURRENT_USER, HKEY_LOCAL_MACHINE,
    KEY_ALL_ACCESS, KEY_READ, REG_EXPAND_SZ, REG_SZ
)

def env_keys(user=True):
    if user:
        root = HKEY_CURRENT_USER
        subkey = 'Environment'
    else:
        root = HKEY_LOCAL_MACHINE
        subkey = 'SYSTEM\\CurrentControlSet\\Control\\Session Manager\\Environment'
    return root, subkey


def get_env(name: str, user=True) -> str:
    root, subkey = env_keys(user)
    key = OpenKey(root, subkey, 0, KEY_READ)
    try:
        value, _ = QueryValueEx(key, name)
    except WindowsError:
        return ''
    return value


def set_env(name: str, value: str) -> None:
    key = OpenKey(HKEY_CURRENT_USER, 'Environment', 0, KEY_ALL_ACCESS)
    SetValueEx(key, name, 0, REG_EXPAND_SZ, value)
    CloseKey(key)
    SendMessage(
        win32con.HWND_BROADCAST, win32con.WM_SETTINGCHANGE, 0, 'Environment')

def remove(paths: 'list[str]', value: str):
    while value in paths:
        paths.remove(value)


def unique(paths: 'list[str]') -> 'list[str]':
    unique = []
    for value in paths:
        if value not in unique:
            unique.append(value)
    return unique
