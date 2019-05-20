export interface NamedBlob {
	name: string,
	blob: Blob
}

export interface HttpRequestOptions {
	getAbortFunc?: (abort: () => void) => void
}

export interface UploadFileHttpRequestOptions extends HttpRequestOptions {
	onUploadProgress?: (progressEvent: ProgressEvent) => void
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
	getAbortFunc: (abort: () => void) => void;
	onUploadProgress?: (progressEvent: ProgressEvent) => void;
	jsonResponseExpected: boolean;
}

export interface GetUriOptions {
	url: string;
	queryStringParams?: { [key: string]: any }
}