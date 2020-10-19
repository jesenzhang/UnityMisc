'''
@Descripttion: https://github.com/jesenzhang/UnityMisc.git
@version: 
@Author: jesen.zhang
@Date: 2020-07-31 10:06:49
LastEditors: jesen.zhang
LastEditTime: 2020-09-11 16:27:03
'''
#!/usr/bin/env python3 
# -*- coding: UTF-8 -*- 
#对图片进行压缩 使用pngquant

import re
import os
import sys
import os.path
import argparse
import platform

   
parser = argparse.ArgumentParser(description='manual to this script')
parser.add_argument('--path', type=str, default = None)
parser.add_argument('--out', type=str, default = None)

args = parser.parse_args()
path = args.path.replace("\\","/")

try:
    path = unicode(path, 'GB2312') # 经过编码处理
except:
    pass # python3 已经移除 unicode，而且默认是 utf8 编码，所以不用转

out=None
if args.out!=None:
    out = args.out.replace("\\","/")
    try:
        out = unicode(out, 'GB2312') # 经过编码处理
    except:
        pass # python3 已经移除 unicode，而且默认是 utf8 编码，所以不用转
    #判断目标是否目录 
    if not os.path.isfile(out):
        if (os.path.exists(out)==False):
            os.makedirs(out)


abs_file=__file__
print("abs path is %s" %(__file__))
abs_dir=abs_file[:abs_file.rfind("\\")]   # windows下用\\分隔路径，linux下用/分隔路径
print("abs path is %s" %(os.path.abspath(sys.argv[0])))

# 得到进程当前工作目录
currentpath = abs_dir;# os.getcwd().replace("\\","/")
systemType = platform.system()
pngquant =  systemType=="Windows" and "../pngquant/pngquant.exe" or systemType=="Mac" and "../pngquant/pngquant" or "../pngquant/pngquant.exe"
pngquant = currentpath+"/"+pngquant
print("pngquant "+pngquant)

guetzli =  systemType=="Windows" and "../guetzli/guetzli_windows_x86-64.exe" or systemType=="Mac" and "../guetzli/guetzli_darwin_x86-64" or "../guetzli/guetzli_windows_x86-64.exe"
guetzli = currentpath+"/"+guetzli
print("guetzli "+guetzli)

#有损压缩
cjpeg =  systemType=="Windows" and "../mozjpeg/windows3.3.1/cjpeg-static.exe" or systemType=="Mac" and "../mozjpeg/mac3.3.1/cjpeg" or  "../mozjpeg/windows3.3.1/cjpeg-static.exe" 
cjpeg = currentpath+"/"+cjpeg
print("cjpeg "+cjpeg)

#无损压缩
jpegtran  =  systemType=="Windows" and "../mozjpeg/windows3.3.1/jpegtran-static.exe" or systemType=="Mac" and "../mozjpeg/mac3.3.1/jpegtran" or "../mozjpeg/windows3.3.1/jpegtran-static.exe"
jpegtran = currentpath+"/"+jpegtran
print("jpegtran "+jpegtran)

# 定义函数 创建目录
def makeDir(outpath):
    (dirPath, file) = os.path.split(outpath)
    if not os.path.isfile(dirPath):
        if (os.path.exists(dirPath)==False):
            os.makedirs(dirPath)

# 定义函数 压缩图片
def compressTexture( filePath ,fileName , outpath ):
    if os.path.isfile(filePath):
        # 匹配文件名正则表达式
        # pat = "^[a-z0-9_]+\.(png||jpg)"
        pat = "(.*)\.(png||jpg)"
        # 进行匹配
        matchObj = re.match(pat, fileName)
        print(fileName)
        if matchObj!=None:
            oldname = fileName.lower()
            oldname = re.sub(' ','',oldname)
            #去除后缀
            imagename= os.path.splitext(oldname)[0]
            suffix= os.path.splitext(oldname)[1]
            print(suffix)

            if suffix==".jpg":
                # print(os.system(guetzli +" --quality 84 --verbose "+ filePath + " " + outpath))
                #print(os.system(jpegtran + " -outfile " + outpath +" "+ filePath ))
                if outpath==filePath:
                     outpath=outpath.replace(fileName,imagename+"_c"+suffix)
                makeDir(outpath)
                print(os.system(cjpeg + " -quality 75 -smooth 0 -baseline -sample 2x2 -quant-table 3 -outfile " + outpath +" "+ filePath))
                #+" -quality 85 -verbose "
            elif suffix==".png":
                makeDir(outpath)
                print(os.system(pngquant +" --force  --skip-if-larger --verbose --speed=1 --quality=45-85 "+ filePath + " --output "+ outpath))
                print(os.system(pngquant +" --force  --skip-if-larger --verbose --ordered --speed=1 --quality=50-90 --ext=.png "+ outpath))
    print(fileName + " Done")
 

if os.path.isfile(path):
    # parent_path = os.path.dirname(path)
    (parent_path, file) = os.path.split(path)
    # pattern = re.compile(r'([^<>/\\\|:""\*\?]+)\.\w+$')
    # filename = pattern.findall(path)
    fileList = [file]
     # 将当前工作目录修改为待修改文件夹的位置
    os.chdir(parent_path)
    print("parent_path "+parent_path)

    filePath = path.replace("\\","/")
    if out==None:
        out=parent_path
    outpath = filePath.replace(parent_path,out).replace("\\","/")

    print("path "+path)
    print("file "+file)
    print("outpath "+outpath)

    compressTexture(path,file,outpath)

else:
    parent_path = path
    fileList = os.listdir(parent_path)
    # 将当前工作目录修改为待修改文件夹的位置
    os.chdir(parent_path)
    print("parent_path "+parent_path)

    for root, dirs, files in os.walk(path):
        print('root_dir:', root)  # 当前目录路径
        print('sub_dirs:', dirs)  # 当前路径下所有子目录
        print('files:', files)  # 当前路径下所有非目录子文件

        for fileName in files:
            filePath = os.path.join(root,fileName).replace("\\","/")
            if out==None:
                out=path
            outpath = filePath.replace(path,out).replace("\\","/")
            print("filePath "+filePath)
            print("outpath "+outpath)
            compressTexture(filePath,fileName,outpath)



# # 遍历文件夹中所有文件
# for fileName in fileList:
#     if os.path.isfile(fileName):
#         # 匹配文件名正则表达式
#         # pat = "^[a-z0-9_]+\.(png||jpg)"
#         pat = "(.*)\.(png||jpg)"
#         # 进行匹配
#         matchObj = re.match(pat, fileName)
#         print(fileName)

#         if matchObj!=None:
#             oldname = fileName.lower()
#             oldname = re.sub(' ','',oldname)
#             #去除后缀
#             imagename= os.path.splitext(oldname)[0]
#             suffix= os.path.splitext(oldname)[1]
#             print(suffix)
#             if suffix==".jpg":
#                 if out==None:
#                     outpath =os.path.join(parent_path,fileName).replace("\\","/")
#                 else:
#                     outpath =os.path.join(out,fileName).replace("\\","/")
#                 print(os.system(guetzli +" --quality 84 --verbose "+ fileName + " " + outpath))
#             elif suffix==".png":
#                 outpath =os.path.join(out,fileName).replace("\\","/")
#                 if out==None:
#                     print(os.system(pngquant +" --force  --skip-if-larger --verbose --speed=1 --quality=45-85  --ext=.png "+ fileName))
#                     print(os.system(pngquant +" --force  --skip-if-larger --verbose --ordered --speed=1 --quality=50-90  --ext=.png "+ fileName))
#                 else:
#                     print(os.system(pngquant +" --force  --skip-if-larger --verbose --speed=1 --quality=45-85 "+ fileName + " --output "+ outpath))
#                     print(os.system(pngquant +" --force  --skip-if-larger --verbose --ordered --speed=1 --quality=50-90 --ext=.png "+ outpath))
#         print(fileName + " Done")
       
sys.stdin.flush()

os.chdir(currentpath)

print("Done")
