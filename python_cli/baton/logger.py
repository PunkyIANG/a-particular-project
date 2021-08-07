from colorama import init, Fore, Back, Style
init()

def log(message : str):
    print(Fore.WHITE)
    print(message)

def log_success(message : str):
    print(Fore.GREEN)
    print(message)

def log_error(message : str):
    print(Fore.RED)
    print(message)

def log_warning(message : str):
    print(Fore.YELLOW)
    print(message)

def log_info(message : str):
    print(Fore.CYAN)
    print(message)

def log_reset():
    print(Style.RESET_ALL)