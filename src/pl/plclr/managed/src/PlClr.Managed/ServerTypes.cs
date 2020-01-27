﻿using System;
using System.Collections.Generic;

namespace PlClr
{
    public static partial class ServerTypes
    {
        private static readonly Dictionary<uint, string> ValueAccessMethodCache = new Dictionary<uint, string>();
        private static readonly Dictionary<uint, Type> TypeCache = new Dictionary<uint, Type>();
        private static readonly Dictionary<uint, TypeAccessInfo> TypeAccessInfoCache = new Dictionary<uint, TypeAccessInfo>();

        internal static TypeInfo GetTypeInfo(uint oid)
            => oid switch
            {
                _ => LookupOrCreateTypeInfo(oid)
            };

        private static TypeInfo LookupOrCreateTypeInfo(uint oid)
        {
            if (TypeInfoCache.TryGetValue(oid, out var value))
                return value;

            var typeInfo = ServerFunctions.GetTypeInfo(oid);
            TypeInfoCache[oid] = typeInfo;
            return typeInfo;
        }

        internal static TypeAccessInfo GeTypeAccessInfo(uint oid)
            => oid switch
            {
                16 => BoolTypeAccessInfo,
                20 => Int64TypeAccessInfo,
                21 => Int16TypeAccessInfo,
                23 => Int32TypeAccessInfo,
                25 => TextTypeAccessInfo,
                26 => OidTypeAccessInfo,
                700 => FloatTypeAccessInfo,
                701 => DoubleTypeAccessInfo,
                2278 => VoidTypeAccessInfo,
                _ => LookupOrCreateTypeAccessInfo(oid)
            };

        private static TypeAccessInfo LookupOrCreateTypeAccessInfo(uint oid)
        {
            if (TypeAccessInfoCache.TryGetValue(oid, out var value))
                return value;

            var typeInfo = GetTypeInfo(oid);
            var typeAccessInfo = CSharpCompiler.CreateTypeAccessInfo(typeInfo);
            TypeAccessInfoCache[oid] = typeAccessInfo;
            return typeAccessInfo;
        }

/*
        public static string GetValueAccessMethodForOid(uint oid)
            => oid switch
            {
                16 => $"{nameof(BackendFunctions)}.{nameof(BackendFunctions.GetBool)}", // bool
                // 17 => typeof(bytea), // bytea
                // 18 => typeof(char), // char
                // 19 => typeof(name), // name
                20 => $"{nameof(BackendFunctions)}.{nameof(BackendFunctions.GetInt64)}", // int8
                21 => $"{nameof(BackendFunctions)}.{nameof(BackendFunctions.GetInt16)}", // int2
                // 22 => typeof(int2vector), // int2vector
                23 => $"{nameof(BackendFunctions)}.{nameof(BackendFunctions.GetInt32)}", // int4
                // 24 => typeof(regproc), // regproc
                25 => $"{nameof(BackendFunctions)}.{nameof(BackendFunctions.GetText)}", // text
                // 26 => typeof(uint), // oid
                // 27 => typeof(tid), // tid
                // 28 => typeof(xid), // xid
                // 29 => typeof(cid), // cid
                // 30 => typeof(oidvector), // oidvector
                // 32 => typeof(pg_ddl_command), // pg_ddl_command
                // 71 => typeof(pg_type), // pg_type
                // 75 => typeof(pg_attribute), // pg_attribute
                // 81 => typeof(pg_proc), // pg_proc
                // 83 => typeof(pg_class), // pg_class
                // 114 => typeof(json), // json
                // 142 => typeof(xml), // xml
                // 143 => typeof(_xml), // _xml
                // 194 => typeof(pg_node_tree), // pg_node_tree
                // 199 => typeof(_json), // _json
                // 269 => typeof(table_am_handler), // table_am_handler
                // 325 => typeof(index_am_handler), // index_am_handler
                // 600 => typeof(point), // point
                // 601 => typeof(lseg), // lseg
                // 602 => typeof(path), // path
                // 603 => typeof(box), // box
                // 604 => typeof(polygon), // polygon
                // 628 => typeof(line), // line
                // 629 => typeof(_line), // _line
                // 650 => typeof(cidr), // cidr
                // 651 => typeof(_cidr), // _cidr
                // 700 => typeof(float), // float4
                // 701 => typeof(double), // float8
                // 705 => typeof(unknown), // unknown
                // 718 => typeof(circle), // circle
                // 719 => typeof(_circle), // _circle
                // 774 => typeof(macaddr8), // macaddr8
                // 775 => typeof(_macaddr8), // _macaddr8
                // 790 => typeof(money), // money
                // 791 => typeof(_money), // _money
                // 829 => typeof(macaddr), // macaddr
                // 869 => typeof(inet), // inet
                // 1000 => typeof(_bool), // _bool
                // 1001 => typeof(_bytea), // _bytea
                // 1002 => typeof(_char), // _char
                // 1003 => typeof(_name), // _name
                // 1005 => typeof(_int2), // _int2
                // 1006 => typeof(_int2vector), // _int2vector
                // 1007 => typeof(_int4), // _int4
                // 1008 => typeof(_regproc), // _regproc
                // 1009 => typeof(_text), // _text
                // 1010 => typeof(_tid), // _tid
                // 1011 => typeof(_xid), // _xid
                // 1012 => typeof(_cid), // _cid
                // 1013 => typeof(_oidvector), // _oidvector
                // 1014 => typeof(_bpchar), // _bpchar
                // 1015 => typeof(_varchar), // _varchar
                // 1016 => typeof(_int8), // _int8
                // 1017 => typeof(_point), // _point
                // 1018 => typeof(_lseg), // _lseg
                // 1019 => typeof(_path), // _path
                // 1020 => typeof(_box), // _box
                // 1021 => typeof(_float4), // _float4
                // 1022 => typeof(_float8), // _float8
                // 1027 => typeof(_polygon), // _polygon
                // 1028 => typeof(_oid), // _oid
                // 1033 => typeof(aclitem), // aclitem
                // 1034 => typeof(_aclitem), // _aclitem
                // 1040 => typeof(_macaddr), // _macaddr
                // 1041 => typeof(_inet), // _inet
                // 1042 => typeof(bpchar), // bpchar
                // 1043 => typeof(varchar), // varchar
                // 1082 => typeof(date), // date
                // 1083 => typeof(time), // time
                // 1114 => typeof(timestamp), // timestamp
                // 1115 => typeof(_timestamp), // _timestamp
                // 1182 => typeof(_date), // _date
                // 1183 => typeof(_time), // _time
                // 1184 => typeof(timestamptz), // timestamptz
                // 1185 => typeof(_timestamptz), // _timestamptz
                // 1186 => typeof(interval), // interval
                // 1187 => typeof(_interval), // _interval
                // 1231 => typeof(_numeric), // _numeric
                // 1263 => typeof(_cstring), // _cstring
                // 1266 => typeof(timetz), // timetz
                // 1270 => typeof(_timetz), // _timetz
                // 1560 => typeof(bit), // bit
                // 1561 => typeof(_bit), // _bit
                // 1562 => typeof(varbit), // varbit
                // 1563 => typeof(_varbit), // _varbit
                // 1700 => typeof(numeric), // numeric
                // 1790 => typeof(refcursor), // refcursor
                // 2201 => typeof(_refcursor), // _refcursor
                // 2202 => typeof(regprocedure), // regprocedure
                // 2203 => typeof(regoper), // regoper
                // 2204 => typeof(regoperator), // regoperator
                // 2205 => typeof(regclass), // regclass
                // 2206 => typeof(regtype), // regtype
                // 2207 => typeof(_regprocedure), // _regprocedure
                // 2208 => typeof(_regoper), // _regoper
                // 2209 => typeof(_regoperator), // _regoperator
                // 2210 => typeof(_regclass), // _regclass
                // 2211 => typeof(_regtype), // _regtype
                // 2249 => typeof(record), // record
                // 2275 => typeof(cstring), // cstring
                // 2276 => typeof(any), // any
                // 2277 => typeof(anyarray), // anyarray
                // 2278 => typeof(void), // void
                // 2279 => typeof(trigger), // trigger
                // 2280 => typeof(language_handler), // language_handler
                // 2281 => typeof(internal), // internal
                // 2282 => typeof(opaque), // opaque
                // 2283 => typeof(anyelement), // anyelement
                // 2287 => typeof(_record), // _record
                // 2776 => typeof(anynonarray), // anynonarray
                // 2949 => typeof(_txid_snapshot), // _txid_snapshot
                // 2950 => typeof(uuid), // uuid
                // 2951 => typeof(_uuid), // _uuid
                // 2970 => typeof(txid_snapshot), // txid_snapshot
                // 3115 => typeof(fdw_handler), // fdw_handler
                // 3220 => typeof(pg_lsn), // pg_lsn
                // 3221 => typeof(_pg_lsn), // _pg_lsn
                // 3310 => typeof(tsm_handler), // tsm_handler
                // 3361 => typeof(pg_ndistinct), // pg_ndistinct
                // 3402 => typeof(pg_dependencies), // pg_dependencies
                // 3500 => typeof(anyenum), // anyenum
                // 3614 => typeof(tsvector), // tsvector
                // 3615 => typeof(tsquery), // tsquery
                // 3642 => typeof(gtsvector), // gtsvector
                // 3643 => typeof(_tsvector), // _tsvector
                // 3644 => typeof(_gtsvector), // _gtsvector
                // 3645 => typeof(_tsquery), // _tsquery
                // 3734 => typeof(regconfig), // regconfig
                // 3735 => typeof(_regconfig), // _regconfig
                // 3769 => typeof(regdictionary), // regdictionary
                // 3770 => typeof(_regdictionary), // _regdictionary
                // 3802 => typeof(jsonb), // jsonb
                // 3807 => typeof(_jsonb), // _jsonb
                // 3831 => typeof(anyrange), // anyrange
                // 3838 => typeof(event_trigger), // event_trigger
                // 3904 => typeof(int4range), // int4range
                // 3905 => typeof(_int4range), // _int4range
                // 3906 => typeof(numrange), // numrange
                // 3907 => typeof(_numrange), // _numrange
                // 3908 => typeof(tsrange), // tsrange
                // 3909 => typeof(_tsrange), // _tsrange
                // 3910 => typeof(tstzrange), // tstzrange
                // 3911 => typeof(_tstzrange), // _tstzrange
                // 3912 => typeof(daterange), // daterange
                // 3913 => typeof(_daterange), // _daterange
                // 3926 => typeof(int8range), // int8range
                // 3927 => typeof(_int8range), // _int8range
                // 4072 => typeof(jsonpath), // jsonpath
                // 4073 => typeof(_jsonpath), // _jsonpath
                // 4089 => typeof(regnamespace), // regnamespace
                // 4090 => typeof(_regnamespace), // _regnamespace
                // 4096 => typeof(regrole), // regrole
                // 4097 => typeof(_regrole), // _regrole
                // 5017 => typeof(pg_mcv_list), // pg_mcv_list
                _ => ValueAccessMethodCache.TryGetValue(oid, out var value)
                    ? value
                    : TryLookupValueAccessMethodForOid(oid, out value)
                        ? value 
                        : throw ServerLog.EReport(
                            SeverityLevel.Error,
                            $"The type with Oid {oid} is currently not supported by PL/CLR.",
                            errorDataType: oid)!
    };

        private static bool TryLookupValueAccessMethodForOid(uint oid, [NotNullWhen(true)] out string? value)
        {
            if (ValueAccessMethodCache.TryGetValue(oid, out value))
                return true;

            if (!TypeInfoCache.TryGetValue(oid, out var ti))
            {
                ti = BackendFunctions.GetTypeInfo(oid);
                TypeInfoCache[oid] = ti;
            }

            var typeAccessInfo = CSharpCompiler.CreateTypeAccessInfo(ti);

            value = $"{typeAccessInfo.AccessMethodType.Name}.{typeAccessInfo.AccessMethodName}";
            ValueAccessMethodCache[oid] = value;
            TypeCache[oid] = typeAccessInfo.AccessMethodType;
            return true;
        }

        public static string GetValueCreationMethodForOid(uint oid)
            => oid switch
            {
                16 => nameof(ServerFunctions.GetDatum), // bool
                // 17 => typeof(bytea), // bytea
                // 18 => typeof(char), // char
                // 19 => typeof(name), // name
                20 => nameof(ServerFunctions.GetDatum), // int8
                21 => nameof(ServerFunctions.GetDatum), // int2
                // 22 => typeof(int2vector), // int2vector
                23 => nameof(ServerFunctions.GetDatum), // int4
                // 24 => typeof(regproc), // regproc
                25 => nameof(ServerFunctions.TextGetDatum), // text
                // 26 => typeof(uint), // oid
                // 27 => typeof(tid), // tid
                // 28 => typeof(xid), // xid
                // 29 => typeof(cid), // cid
                // 30 => typeof(oidvector), // oidvector
                // 32 => typeof(pg_ddl_command), // pg_ddl_command
                // 71 => typeof(pg_type), // pg_type
                // 75 => typeof(pg_attribute), // pg_attribute
                // 81 => typeof(pg_proc), // pg_proc
                // 83 => typeof(pg_class), // pg_class
                // 114 => typeof(json), // json
                // 142 => typeof(xml), // xml
                // 143 => typeof(_xml), // _xml
                // 194 => typeof(pg_node_tree), // pg_node_tree
                // 199 => typeof(_json), // _json
                // 269 => typeof(table_am_handler), // table_am_handler
                // 325 => typeof(index_am_handler), // index_am_handler
                // 600 => typeof(point), // point
                // 601 => typeof(lseg), // lseg
                // 602 => typeof(path), // path
                // 603 => typeof(box), // box
                // 604 => typeof(polygon), // polygon
                // 628 => typeof(line), // line
                // 629 => typeof(_line), // _line
                // 650 => typeof(cidr), // cidr
                // 651 => typeof(_cidr), // _cidr
                // 700 => typeof(float), // float4
                // 701 => typeof(double), // float8
                // 705 => typeof(unknown), // unknown
                // 718 => typeof(circle), // circle
                // 719 => typeof(_circle), // _circle
                // 774 => typeof(macaddr8), // macaddr8
                // 775 => typeof(_macaddr8), // _macaddr8
                // 790 => typeof(money), // money
                // 791 => typeof(_money), // _money
                // 829 => typeof(macaddr), // macaddr
                // 869 => typeof(inet), // inet
                // 1000 => typeof(_bool), // _bool
                // 1001 => typeof(_bytea), // _bytea
                // 1002 => typeof(_char), // _char
                // 1003 => typeof(_name), // _name
                // 1005 => typeof(_int2), // _int2
                // 1006 => typeof(_int2vector), // _int2vector
                // 1007 => typeof(_int4), // _int4
                // 1008 => typeof(_regproc), // _regproc
                // 1009 => typeof(_text), // _text
                // 1010 => typeof(_tid), // _tid
                // 1011 => typeof(_xid), // _xid
                // 1012 => typeof(_cid), // _cid
                // 1013 => typeof(_oidvector), // _oidvector
                // 1014 => typeof(_bpchar), // _bpchar
                // 1015 => typeof(_varchar), // _varchar
                // 1016 => typeof(_int8), // _int8
                // 1017 => typeof(_point), // _point
                // 1018 => typeof(_lseg), // _lseg
                // 1019 => typeof(_path), // _path
                // 1020 => typeof(_box), // _box
                // 1021 => typeof(_float4), // _float4
                // 1022 => typeof(_float8), // _float8
                // 1027 => typeof(_polygon), // _polygon
                // 1028 => typeof(_oid), // _oid
                // 1033 => typeof(aclitem), // aclitem
                // 1034 => typeof(_aclitem), // _aclitem
                // 1040 => typeof(_macaddr), // _macaddr
                // 1041 => typeof(_inet), // _inet
                // 1042 => typeof(bpchar), // bpchar
                // 1043 => typeof(varchar), // varchar
                // 1082 => typeof(date), // date
                // 1083 => typeof(time), // time
                // 1114 => typeof(timestamp), // timestamp
                // 1115 => typeof(_timestamp), // _timestamp
                // 1182 => typeof(_date), // _date
                // 1183 => typeof(_time), // _time
                // 1184 => typeof(timestamptz), // timestamptz
                // 1185 => typeof(_timestamptz), // _timestamptz
                // 1186 => typeof(interval), // interval
                // 1187 => typeof(_interval), // _interval
                // 1231 => typeof(_numeric), // _numeric
                // 1263 => typeof(_cstring), // _cstring
                // 1266 => typeof(timetz), // timetz
                // 1270 => typeof(_timetz), // _timetz
                // 1560 => typeof(bit), // bit
                // 1561 => typeof(_bit), // _bit
                // 1562 => typeof(varbit), // varbit
                // 1563 => typeof(_varbit), // _varbit
                // 1700 => typeof(numeric), // numeric
                // 1790 => typeof(refcursor), // refcursor
                // 2201 => typeof(_refcursor), // _refcursor
                // 2202 => typeof(regprocedure), // regprocedure
                // 2203 => typeof(regoper), // regoper
                // 2204 => typeof(regoperator), // regoperator
                // 2205 => typeof(regclass), // regclass
                // 2206 => typeof(regtype), // regtype
                // 2207 => typeof(_regprocedure), // _regprocedure
                // 2208 => typeof(_regoper), // _regoper
                // 2209 => typeof(_regoperator), // _regoperator
                // 2210 => typeof(_regclass), // _regclass
                // 2211 => typeof(_regtype), // _regtype
                // 2249 => typeof(record), // record
                // 2275 => typeof(cstring), // cstring
                // 2276 => typeof(any), // any
                // 2277 => typeof(anyarray), // anyarray
                // 2278 => typeof(void), // void
                // 2279 => typeof(trigger), // trigger
                // 2280 => typeof(language_handler), // language_handler
                // 2281 => typeof(internal), // internal
                // 2282 => typeof(opaque), // opaque
                // 2283 => typeof(anyelement), // anyelement
                // 2287 => typeof(_record), // _record
                // 2776 => typeof(anynonarray), // anynonarray
                // 2949 => typeof(_txid_snapshot), // _txid_snapshot
                // 2950 => typeof(uuid), // uuid
                // 2951 => typeof(_uuid), // _uuid
                // 2970 => typeof(txid_snapshot), // txid_snapshot
                // 3115 => typeof(fdw_handler), // fdw_handler
                // 3220 => typeof(pg_lsn), // pg_lsn
                // 3221 => typeof(_pg_lsn), // _pg_lsn
                // 3310 => typeof(tsm_handler), // tsm_handler
                // 3361 => typeof(pg_ndistinct), // pg_ndistinct
                // 3402 => typeof(pg_dependencies), // pg_dependencies
                // 3500 => typeof(anyenum), // anyenum
                // 3614 => typeof(tsvector), // tsvector
                // 3615 => typeof(tsquery), // tsquery
                // 3642 => typeof(gtsvector), // gtsvector
                // 3643 => typeof(_tsvector), // _tsvector
                // 3644 => typeof(_gtsvector), // _gtsvector
                // 3645 => typeof(_tsquery), // _tsquery
                // 3734 => typeof(regconfig), // regconfig
                // 3735 => typeof(_regconfig), // _regconfig
                // 3769 => typeof(regdictionary), // regdictionary
                // 3770 => typeof(_regdictionary), // _regdictionary
                // 3802 => typeof(jsonb), // jsonb
                // 3807 => typeof(_jsonb), // _jsonb
                // 3831 => typeof(anyrange), // anyrange
                // 3838 => typeof(event_trigger), // event_trigger
                // 3904 => typeof(int4range), // int4range
                // 3905 => typeof(_int4range), // _int4range
                // 3906 => typeof(numrange), // numrange
                // 3907 => typeof(_numrange), // _numrange
                // 3908 => typeof(tsrange), // tsrange
                // 3909 => typeof(_tsrange), // _tsrange
                // 3910 => typeof(tstzrange), // tstzrange
                // 3911 => typeof(_tstzrange), // _tstzrange
                // 3912 => typeof(daterange), // daterange
                // 3913 => typeof(_daterange), // _daterange
                // 3926 => typeof(int8range), // int8range
                // 3927 => typeof(_int8range), // _int8range
                // 4072 => typeof(jsonpath), // jsonpath
                // 4073 => typeof(_jsonpath), // _jsonpath
                // 4089 => typeof(regnamespace), // regnamespace
                // 4090 => typeof(_regnamespace), // _regnamespace
                // 4096 => typeof(regrole), // regrole
                // 4097 => typeof(_regrole), // _regrole
                // 5017 => typeof(pg_mcv_list), // pg_mcv_list
                _ => throw ServerLog.EReport(
                    SeverityLevel.Error,
                    errorMessageInternal: $"The type with Oid {oid} is currently not supported by PL/CLR.",
                    errorDataType: oid)!
    };

        public static Type GetTypeForOid(uint oid)
            => oid switch
            {
                16 => typeof(bool), // bool
                // 17 => typeof(bytea), // bytea
                // 18 => typeof(char), // char
                // 19 => typeof(name), // name
                20 => typeof(long), // int8
                21 => typeof(short), // int2
                // 22 => typeof(int2vector), // int2vector
                23 => typeof(int), // int4
                // 24 => typeof(regproc), // regproc
                25 => typeof(string), // text
                26 => typeof(uint), // oid
                // 27 => typeof(tid), // tid
                // 28 => typeof(xid), // xid
                // 29 => typeof(cid), // cid
                // 30 => typeof(oidvector), // oidvector
                // 32 => typeof(pg_ddl_command), // pg_ddl_command
                // 71 => typeof(pg_type), // pg_type
                // 75 => typeof(pg_attribute), // pg_attribute
                // 81 => typeof(pg_proc), // pg_proc
                // 83 => typeof(pg_class), // pg_class
                // 114 => typeof(json), // json
                // 142 => typeof(xml), // xml
                // 143 => typeof(_xml), // _xml
                // 194 => typeof(pg_node_tree), // pg_node_tree
                // 199 => typeof(_json), // _json
                // 269 => typeof(table_am_handler), // table_am_handler
                // 325 => typeof(index_am_handler), // index_am_handler
                // 600 => typeof(point), // point
                // 601 => typeof(lseg), // lseg
                // 602 => typeof(path), // path
                // 603 => typeof(box), // box
                // 604 => typeof(polygon), // polygon
                // 628 => typeof(line), // line
                // 629 => typeof(_line), // _line
                // 650 => typeof(cidr), // cidr
                // 651 => typeof(_cidr), // _cidr
                700 => typeof(float), // float4
                701 => typeof(double), // float8
                // 705 => typeof(unknown), // unknown
                // 718 => typeof(circle), // circle
                // 719 => typeof(_circle), // _circle
                // 774 => typeof(macaddr8), // macaddr8
                // 775 => typeof(_macaddr8), // _macaddr8
                // 790 => typeof(money), // money
                // 791 => typeof(_money), // _money
                // 829 => typeof(macaddr), // macaddr
                // 869 => typeof(inet), // inet
                // 1000 => typeof(_bool), // _bool
                // 1001 => typeof(_bytea), // _bytea
                // 1002 => typeof(_char), // _char
                // 1003 => typeof(_name), // _name
                // 1005 => typeof(_int2), // _int2
                // 1006 => typeof(_int2vector), // _int2vector
                // 1007 => typeof(_int4), // _int4
                // 1008 => typeof(_regproc), // _regproc
                // 1009 => typeof(_text), // _text
                // 1010 => typeof(_tid), // _tid
                // 1011 => typeof(_xid), // _xid
                // 1012 => typeof(_cid), // _cid
                // 1013 => typeof(_oidvector), // _oidvector
                // 1014 => typeof(_bpchar), // _bpchar
                // 1015 => typeof(_varchar), // _varchar
                // 1016 => typeof(_int8), // _int8
                // 1017 => typeof(_point), // _point
                // 1018 => typeof(_lseg), // _lseg
                // 1019 => typeof(_path), // _path
                // 1020 => typeof(_box), // _box
                // 1021 => typeof(_float4), // _float4
                // 1022 => typeof(_float8), // _float8
                // 1027 => typeof(_polygon), // _polygon
                // 1028 => typeof(_oid), // _oid
                // 1033 => typeof(aclitem), // aclitem
                // 1034 => typeof(_aclitem), // _aclitem
                // 1040 => typeof(_macaddr), // _macaddr
                // 1041 => typeof(_inet), // _inet
                // 1042 => typeof(bpchar), // bpchar
                // 1043 => typeof(varchar), // varchar
                // 1082 => typeof(date), // date
                // 1083 => typeof(time), // time
                // 1114 => typeof(timestamp), // timestamp
                // 1115 => typeof(_timestamp), // _timestamp
                // 1182 => typeof(_date), // _date
                // 1183 => typeof(_time), // _time
                // 1184 => typeof(timestamptz), // timestamptz
                // 1185 => typeof(_timestamptz), // _timestamptz
                // 1186 => typeof(interval), // interval
                // 1187 => typeof(_interval), // _interval
                // 1231 => typeof(_numeric), // _numeric
                // 1263 => typeof(_cstring), // _cstring
                // 1266 => typeof(timetz), // timetz
                // 1270 => typeof(_timetz), // _timetz
                // 1560 => typeof(bit), // bit
                // 1561 => typeof(_bit), // _bit
                // 1562 => typeof(varbit), // varbit
                // 1563 => typeof(_varbit), // _varbit
                // 1700 => typeof(numeric), // numeric
                // 1790 => typeof(refcursor), // refcursor
                // 2201 => typeof(_refcursor), // _refcursor
                // 2202 => typeof(regprocedure), // regprocedure
                // 2203 => typeof(regoper), // regoper
                // 2204 => typeof(regoperator), // regoperator
                // 2205 => typeof(regclass), // regclass
                // 2206 => typeof(regtype), // regtype
                // 2207 => typeof(_regprocedure), // _regprocedure
                // 2208 => typeof(_regoper), // _regoper
                // 2209 => typeof(_regoperator), // _regoperator
                // 2210 => typeof(_regclass), // _regclass
                // 2211 => typeof(_regtype), // _regtype
                // 2249 => typeof(record), // record
                // 2275 => typeof(cstring), // cstring
                // 2276 => typeof(any), // any
                // 2277 => typeof(anyarray), // anyarray
                2278 => typeof(void), // void
                // 2279 => typeof(trigger), // trigger
                // 2280 => typeof(language_handler), // language_handler
                // 2281 => typeof(internal), // internal
                // 2282 => typeof(opaque), // opaque
                // 2283 => typeof(anyelement), // anyelement
                // 2287 => typeof(_record), // _record
                // 2776 => typeof(anynonarray), // anynonarray
                // 2949 => typeof(_txid_snapshot), // _txid_snapshot
                // 2950 => typeof(uuid), // uuid
                // 2951 => typeof(_uuid), // _uuid
                // 2970 => typeof(txid_snapshot), // txid_snapshot
                // 3115 => typeof(fdw_handler), // fdw_handler
                // 3220 => typeof(pg_lsn), // pg_lsn
                // 3221 => typeof(_pg_lsn), // _pg_lsn
                // 3310 => typeof(tsm_handler), // tsm_handler
                // 3361 => typeof(pg_ndistinct), // pg_ndistinct
                // 3402 => typeof(pg_dependencies), // pg_dependencies
                // 3500 => typeof(anyenum), // anyenum
                // 3614 => typeof(tsvector), // tsvector
                // 3615 => typeof(tsquery), // tsquery
                // 3642 => typeof(gtsvector), // gtsvector
                // 3643 => typeof(_tsvector), // _tsvector
                // 3644 => typeof(_gtsvector), // _gtsvector
                // 3645 => typeof(_tsquery), // _tsquery
                // 3734 => typeof(regconfig), // regconfig
                // 3735 => typeof(_regconfig), // _regconfig
                // 3769 => typeof(regdictionary), // regdictionary
                // 3770 => typeof(_regdictionary), // _regdictionary
                // 3802 => typeof(jsonb), // jsonb
                // 3807 => typeof(_jsonb), // _jsonb
                // 3831 => typeof(anyrange), // anyrange
                // 3838 => typeof(event_trigger), // event_trigger
                // 3904 => typeof(int4range), // int4range
                // 3905 => typeof(_int4range), // _int4range
                // 3906 => typeof(numrange), // numrange
                // 3907 => typeof(_numrange), // _numrange
                // 3908 => typeof(tsrange), // tsrange
                // 3909 => typeof(_tsrange), // _tsrange
                // 3910 => typeof(tstzrange), // tstzrange
                // 3911 => typeof(_tstzrange), // _tstzrange
                // 3912 => typeof(daterange), // daterange
                // 3913 => typeof(_daterange), // _daterange
                // 3926 => typeof(int8range), // int8range
                // 3927 => typeof(_int8range), // _int8range
                // 4072 => typeof(jsonpath), // jsonpath
                // 4073 => typeof(_jsonpath), // _jsonpath
                // 4089 => typeof(regnamespace), // regnamespace
                // 4090 => typeof(_regnamespace), // _regnamespace
                // 4096 => typeof(regrole), // regrole
                // 4097 => typeof(_regrole), // _regrole
                // 5017 => typeof(pg_mcv_list), // pg_mcv_list
                _ => TypeCache.TryGetValue(oid, out var value)
                    ? value
                    : throw ServerLog.EReport(
                        SeverityLevel.Error,
                        $"The type with Oid {oid} is currently not supported by PL/CLR.",
                        errorDataType: oid)!
            };
            */
    }
}
