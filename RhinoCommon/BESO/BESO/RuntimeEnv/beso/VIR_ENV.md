# BESO on commandline

## Prepare for BESO

A BESO requirement is an `INP` file needed to do FEA.

## Config BESO

The `beso_conf.py` file is modified. These variables:

```python
path_calculix = "C:\\Users\\m3\\Downloads\\calculix_2.21_4win\\ccx_static.exe" # path to the CalculiX solver
file_name = "C:\\Users\\m3\\AppData\\Local\\Temp\\3473023704f\\result.inp"  # file with prepared linear static analysis
cpu_cores = 1  # 0 - use all processor cores, N - will use N number of processor cores
```

## Run BESO

```batch
cd C:\Users\m3\repos\beso 				# Go to BESO repository folder: https://github.com/calculix/beso
pip install virtualenv --user 			# Install virtual env, also: https://github.com/pypa/pip/issues/11845
python -m venv virtual_env 				# Create a virtual env
virtual_env\Scripts\activate.bat 		# Activate the virtual env
pip install numpy 						# Install a dependency
pip install matplotlib 					# Install another dependency
python beso_main.py 					# Run BESO. It uses `beso_conf.py` config file.
```

## Visualize BESO

For the square benchmark and current config, BESO went through 62 iterations. Looks like the last iteration makes sense 📎

```batch
c:\Users\m3\Downloads\calculix_2.21_4win\cgx_STATIC.exe -c file062_state1.inp
```

## BESO requirements

The `requirements.txt` file is created by `pip freeze > requirements.txt` after activating the virtual env.

They can be installed by `pip install -r requirements.txt` after activating the virtual env.
