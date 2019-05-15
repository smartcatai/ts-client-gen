export interface NamedBlob {
	name: string,
	blob: Blob
}

export interface HttpRequestOptions {
	cancelToken?: CancelToken
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
	cancelToken?: CancelToken;
	onUploadProgress?: (progressEvent: ProgressEvent) => void;
	jsonResponseExpected: boolean;
}

export interface CancelToken {
	promise: Promise<{ message: string }>;
	reason?: { message: string };
	throwIfRequested(): void;
}

export interface GetUriOptions {
	url: string;
	queryStringParams?: { [key: string]: any }
}