from setuptools import setup

setup(
    name='baton',
    version='0.0.1',
    py_modules=['baton'],
    install_requires=[
        'click>=8'
    ],
    entry_points={
        'console_scripts': [
            'baton = baton:cli'
        ],
    },
)