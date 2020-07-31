'''
@Descripttion: https://github.com/jesenzhang/UnityMisc.git
@version: 
@Author: jesen.zhang
@Date: 2020-07-16 08:40:39
@LastEditors: jesen.zhang
@LastEditTime: 2020-07-31 11:49:11
'''
#!/usr/bin/env python3 
# -*- coding: UTF-8 -*- 
#适用于导入资源到unity前对图片命名进行检查 导入到unity的资源 使用AssetPostProcess处理

import re
import os
import os.path
import sys
import argparse

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
    if os.path.isfile(fileName):
        # 匹配文件名正则表达式
        pat = "^[a-z0-9_]+\.(png||jpg)"
        # 进行匹配
        matchObj = re.match(pat, fileName)
        if matchObj==None:
            oldname = fileName.lower()
            #去除后缀
            imagename= os.path.splitext(oldname)[0]
            suffix= os.path.splitext(oldname)[1]
            #非法字符替换
            fullname = re.sub('[^a-z0-9_]','_',imagename) + suffix
            # 文件重新命名
            os.rename(fileName, fullname)
            # 输出此文件夹中包含的文件名称
            print("修改前：" + fileName +" 修改后：" + fullname)

sys.stdin.flush()

os.chdir(currentpath)

print("Done")