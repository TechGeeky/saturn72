//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Mimas.DbModel
{
    using System;
    using System.Collections.Generic;
    
    public partial class RefreshToken
    {
        public long Id { get; set; }
        public string ClientId { get; set; }
        public string Hash { get; set; }
        public string ProtectedTicket { get; set; }
        public System.DateTime IssuedOnUtc { get; set; }
        public System.DateTime ExpiresOnUtc { get; set; }
        public byte[] RowVersion { get; set; }
    }
}