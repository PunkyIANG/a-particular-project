from setuptools import setup

setup(
    name='baton',
    version='0.0.1',
    py_modules=['baton', 'registry_hijacking', 'git_hooks'],
    install_requires=[
        'click>=8.0',
        'pywin32>=300',
        'GitPython>=3.1'
    ],
    entry_points={
        'console_scripts': [
            'baton = baton:cli'
        ],
    },
)