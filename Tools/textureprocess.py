'''
@Descripttion: https://github.com/jesenzhang/UnityMisc.git
@version: 
@Author: jesen.zhang
@Date: 2020-07-31 11:22:25
@LastEditors: jesen.zhang
@LastEditTime: 2020-07-31 13:03:10
'''
#!/usr/bin/env python3 
# -*- coding: UTF-8 -*- 

import os
import sys
import os.path

#原始图片资源的目录
orignalPath = 'D:/Projects/UnityMisc/Texture/'
#导出的资源目录
outPath = 'D:/Projects/UnityMisc/Assets/Res/Texture/'

zipFilePath = 'D:/Projects/UnityMisc/Assets/Res/RecognitionImage.bytes'


# 得到进程当前工作目录
currentpath = os.getcwd()


for root, dirs, files in os.walk(orignalPath):
    print('root_dir:', root)  # 当前目录路径
    print('sub_dirs:', dirs)  # 当前路径下所有子目录
    print('files:', files)  # 当前路径下所有非目录子文件
    os.chdir(os.path.join(currentpath,"python"))
   # checkname = ('python ./texture_checkname.py --path '+root)
   # os.system(checkname)

    routp = root.replace(orignalPath,outPath)
    resize = ('python ./ImageResize.py --path '+root +" --mode 1 --width 25 --height 25 --outPath " + routp)
    os.system(resize)
    compress = ('python ./texture_compress.py --path '+routp +" --out " + routp)
    os.system(compress)

os.chdir(os.path.join(currentpath,"python"))

zip = ('python ./ZipTool.py --path '+outPath +" --out " + zipFilePath)
os.system(zip)

sys.stdin.flush()
os.chdir(currentpath)

print("Done")