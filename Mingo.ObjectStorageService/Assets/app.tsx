import "vite/modulepreload-polyfill";
import "./index.css";

const uploadFile = async (path: string, file: File, id: string) => {
  try {
    let body: BodyInit = file;

    const headers: Record<string, string> = {
      "Content-Type": "application/octet-stream",
      "MINGO-OSS-Content-Type": file.type,
      "MINGO-OSS-File-Name": file.name,
      "MINGO-OSS-Size": file.size.toString(),
    };

    const cancel = new AbortController();
    const timeoutId = setTimeout(() => cancel.abort(), 1200);

    const response = await fetch(path, {
      method: "PUT",
      headers,
      body,
      signal: cancel.signal,
    });

    if (timeoutId) {
      clearTimeout(timeoutId);
    }

    if (!response.ok) {
      throw new Error(`上传失败: ${response.status} ${response.statusText}`);
    }

    return {
      success: true,
      message: "文件上传成功",
      url: path,
      status: response.status,
    };
  } catch (error) {
    return {
      success: false,
      message: error instanceof Error ? error.message : "文件上传失败",
      status:
        error instanceof Error && error.message === "AbortError" ? 400 : 500,
    };
  }
};
