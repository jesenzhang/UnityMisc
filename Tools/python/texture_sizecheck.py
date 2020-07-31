'''
@Descripttion: https://github.com/jesenzhang/UnityMisc.git
@version: 
@Author: jesen.zhang
@Date: 2020-07-31 14:52:27
@LastEditors: jesen.zhang
@LastEditTime: 2020-07-31 15:17:27
'''
#!/usr/bin/env python3 
# -*- coding: UTF-8 -*- 
#适用于导入资源到unity前对图片命名进行检查 导入到unity的资源 使用AssetPostProcess处理

import re
import os
import os.path
import sys
import argparse
import platform
import numpy as np
from PIL import Image

parser = argparse.ArgumentParser(description='manual to this script')
parser.add_argument('--path', type=str, default = None)
args = parser.parse_args()

fileList = os.listdir(args.path)

# 得到进程当前工作目录
currentpath = os.getcwd()
# 将当前工作目录修改为待修改文件夹的位置
os.chdir(args.path)
# 遍历文件夹中所有文件
for fileName in fileList:
    img = Image.open(fileName)
    print(fileName + ' 宽：%d,高：%d'%(img.size[0],img.size[1]))

sys.stdin.flush()

os.chdir(currentpath)

print("Done")