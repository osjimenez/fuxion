namespace Fuxion;

public abstract class UriKeyException(string message) : FuxionException(message);
public class UriKeyNotFoundException(string message) : UriKeyException(message);
public class UriKeyBypassedException(string message) : UriKeyException(message);
public class UriKeySealedException(string message) : UriKeyException(message);
public abstract class UriKeyFormatException(string message) : UriKeyException(message);
public class UriKeyPathException(string message) : UriKeyFormatException(message);
public class UriKeySemanticVersionException(string message) : UriKeyFormatException(message);
public class UriKeyUserInfoException(string message) : UriKeyFormatException(message);
public class UriKeyFragmentException(string message) : UriKeyFormatException(message);
public class UriKeyParameterException(string message) : UriKeyFormatException(message);