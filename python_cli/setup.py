from setuptools import setup

setup(
    name='baton',
    version='0.0.1',
    packages=['baton'],
    install_requires=[
        'click>=8.0',
        'pywin32>=300',
        'GitPython>=3.1',
        'colorama>=0.4.4'
    ],
    entry_points={
        'console_scripts': [
            'baton = baton.baton:cli'
        ],
    },
)