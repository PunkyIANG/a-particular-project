from setuptools import setup

setup(
    name='baton',
    version='0.0.1',
    py_modules=['baton', 'registry_hijacking'],
    install_requires=[
        'click>=8',
        'pywin32>=300'
    ],
    entry_points={
        'console_scripts': [
            'baton = baton:cli'
        ],
    },
)