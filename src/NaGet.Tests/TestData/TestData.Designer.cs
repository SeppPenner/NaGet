﻿//------------------------------------------------------------------------------
// <auto-generated>
//     Dieser Code wurde von einem Tool generiert.
//     Laufzeitversion:4.0.30319.42000
//
//     Änderungen an dieser Datei können falsches Verhalten verursachen und gehen verloren, wenn
//     der Code erneut generiert wird.
// </auto-generated>
//------------------------------------------------------------------------------

namespace NaGet.Tests {
    using System;
    
    
    /// <summary>
    ///   Eine stark typisierte Ressourcenklasse zum Suchen von lokalisierten Zeichenfolgen usw.
    /// </summary>
    // Diese Klasse wurde von der StronglyTypedResourceBuilder automatisch generiert
    // -Klasse über ein Tool wie ResGen oder Visual Studio automatisch generiert.
    // Um einen Member hinzuzufügen oder zu entfernen, bearbeiten Sie die .ResX-Datei und führen dann ResGen
    // mit der /str-Option erneut aus, oder Sie erstellen Ihr VS-Projekt neu.
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "17.0.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    internal class TestData {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal TestData() {
        }
        
        /// <summary>
        ///   Gibt die zwischengespeicherte ResourceManager-Instanz zurück, die von dieser Klasse verwendet wird.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("NaGet.Tests.TestData.TestData", typeof(TestData).Assembly);
                    resourceMan = temp;
                }
                return resourceMan;
            }
        }
        
        /// <summary>
        ///   Überschreibt die CurrentUICulture-Eigenschaft des aktuellen Threads für alle
        ///   Ressourcenzuordnungen, die diese stark typisierte Ressourcenklasse verwenden.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Globalization.CultureInfo Culture {
            get {
                return resourceCulture;
            }
            set {
                resourceCulture = value;
            }
        }
        
        /// <summary>
        ///   Sucht eine lokalisierte Zeichenfolge, die {&quot;version&quot;:&quot;3.0.0&quot;,&quot;resources&quot;:[{&quot;@id&quot;:&quot;http://localhost/api/v2/package&quot;,&quot;@type&quot;:&quot;PackagePublish/2.0.0&quot;},{&quot;@id&quot;:&quot;http://localhost/api/v2/symbol&quot;,&quot;@type&quot;:&quot;SymbolPackagePublish/4.9.0&quot;},{&quot;@id&quot;:&quot;http://localhost/v3/search&quot;,&quot;@type&quot;:&quot;SearchQueryService&quot;},{&quot;@id&quot;:&quot;http://localhost/v3/search&quot;,&quot;@type&quot;:&quot;SearchQueryService/3.0.0-beta&quot;},{&quot;@id&quot;:&quot;http://localhost/v3/search&quot;,&quot;@type&quot;:&quot;SearchQueryService/3.0.0-rc&quot;},{&quot;@id&quot;:&quot;http://localhost/v3/registration&quot;,&quot;@type&quot;:&quot;RegistrationsBaseUrl&quot;},{&quot;@id&quot;:&quot;http://localhost/v3/registrat [Rest der Zeichenfolge wurde abgeschnitten]&quot;; ähnelt.
        /// </summary>
        internal static string ServiceIndex {
            get {
                return ResourceManager.GetString("ServiceIndex", resourceCulture);
            }
        }
    }
}
