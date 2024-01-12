export interface NamedBlob {
	name: string;
	blob: Blob;
}

export interface HttpRequestOptions {
	getAbortFunc?: (abort: () => void) => void;
	headers?: { [key: string]: string };
}

export interface UploadFileHttpRequestOptions extends HttpRequestOptions {
	onUploadProgress?: (progressEvent: ProgressEvent) => void;
	timeout?: number;
}

export type Method =
	| 'get'
	| 'delete'
	| 'post'
	| 'put'
	| 'patch'

export interface RequestOptions extends GetUriOptions {
	method: Method;
	requestBody?: any,
	getAbortFunc?: (abort: () => void) => void;
	onUploadProgress?: (progressEvent: ProgressEvent) => void;
	jsonResponseExpected: boolean;
	timeout?: number;
	headers?: { [key: string]: string };
}

export interface GetUriOptions {
	url: string;
	queryStringParams?: { [key: string]: any }
}