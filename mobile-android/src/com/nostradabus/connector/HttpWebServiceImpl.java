package com.nostradabus.connector;

import java.io.IOException;
import java.io.InputStream;
import java.util.ArrayList;
import java.util.HashMap;
import java.util.LinkedList;
import java.util.List;
import java.util.Map;

import org.apache.http.HttpResponse;
import org.apache.http.NameValuePair;
import org.apache.http.StatusLine;
import org.apache.http.auth.AuthenticationException;
import org.apache.http.client.ClientProtocolException;
import org.apache.http.client.methods.HttpRequestBase;
import org.apache.http.impl.client.DefaultHttpClient;
import org.apache.http.message.BasicNameValuePair;
import org.apache.http.params.HttpConnectionParams;

import android.util.Log;

import com.nostradabus.connector.parser.ParseResult;
import com.nostradabus.connector.parser.WebServiceParser;
import com.nostradabus.service.handlers.ExceptionHandler;


public abstract class HttpWebServiceImpl implements WebService {

	private String method;
	private String url;
	protected List<NameValuePair> headers = new ArrayList<NameValuePair>();
	protected Map<String, Object> paramsMap;
	protected List<Object> paramsList;
	private WebServiceParser<?> parser;
	
	private static final int HTTP_200 = 200;
	private static final int HTTP_401 = 401;	
	private static final int TIMEOUT = 30000;
	
	public HttpWebServiceImpl() {
		paramsMap = new HashMap<String, Object>();
		paramsList = new LinkedList<Object>();
	}
	
	@Override
	public ParseResult execute() throws AuthenticationException, Exception {
		try {
			DefaultHttpClient client = new DefaultHttpClient();
			HttpConnectionParams.setSoTimeout(client.getParams(), TIMEOUT);
			HttpConnectionParams.setConnectionTimeout(client.getParams(), TIMEOUT);

			HttpRequestBase method = makeHttpMethod();
			
			HttpResponse res = client.execute(method);
			final InputStream inputStream = res.getEntity().getContent();
			
			StatusLine statusLine = res.getStatusLine();
			
			Log.i("<-HttpWebServiceImpl->", "status code   = "+statusLine.getStatusCode());
			Log.i("<-HttpWebServiceImpl->", "response length   = "+res.getEntity().getContentLength());
			
			
			if (statusLine.getStatusCode() != HTTP_200) {
				if (statusLine.getStatusCode() == HTTP_401) {
					throw new AuthenticationException();
				}
				//Show error message
				return null;
			}
			ParseResult result = null;
			if (null != this.getParser()) {
				result =  this.getParser().parse(inputStream);
			}
			
			return result;
		} catch (ClientProtocolException e) {
			e.printStackTrace();
			throw new RuntimeException(e);
		} catch (IOException e) {
			ExceptionHandler.exceptionThrower(e);
		}
		
		return null;
	}

	private WebServiceParser<?> getParser() {
		return parser;
	}

	@Override
	public void addParameter(String name, Object value) {
		paramsMap.put(name, value);
	}

	@Override
	public void addParameter(Object value) {
		paramsList.add(value);
	}
	
	@Override
	public void addHeader(String name, String value) {
		headers.add(new BasicNameValuePair(name, value));
	}

	protected abstract HttpRequestBase makeHttpMethod();
	
	protected String getUri() {
		StringBuffer uri = new StringBuffer(this.url);
		uri.append(this.method);
		return uri.toString();
	}
	
	public String getUrl() {
		StringBuffer urlAndMethod = new StringBuffer(url);
		if (!url.endsWith("/")) {
			urlAndMethod.append("/");	
		}
		urlAndMethod.append(method);
		return urlAndMethod.toString();
	}

	public void setUrl(String url) {
		this.url = url;
	}

	public void setParser(WebServiceParser<?> parser) {
		this.parser = parser;
	}

	public String getMethod() {
		return method;
	}

	public void setMethod(String method) {
		this.method = method;
	}

}