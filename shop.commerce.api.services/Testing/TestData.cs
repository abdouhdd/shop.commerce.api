using shop.commerce.api.domain.Request;
using shop.commerce.api.domain.Entities;
using System;
using System.Collections.Generic;

namespace shop.commerce.api.services.Testing
{
    public class TestData
    {
        public List<Category> Categories
        {
            get
            {
                return new List<Category>
                {
                    new Category
                    {
                        Id = 1,
                        Name = "Accessoire Auto Moto",
                        Active = true,
                    },
                    new Category
                    {
                        Id = 2,
                        Name = "Informatique",
                        Active = true,
                    },
                    new Category
                    {
                        Id = 3,
                        Name = "TV & Hi Tech",
                        Active = true,
                    },
                    new Category
                    {
                        Id = 4,
                        Name = "Alimentation et batterie",
                        Active = true,
                    },
                    new Category
                    {
                        Id = 5,
                        Name = "Autres Alimentation & Battery",
                        Active = true,
                    },
                    new Category
                    {
                        Id = 6,
                        Name = "Accessoires PC",
                        Active = true,
                    },
                    new Category
                    {
                        Id = 7,
                        Name = "Ordinateurs & Imprimantes",
                        Active = true,
                    },
                    new Category
                    {
                        Id = 8,
                        Name = "Logiciel",
                        Active = true,
                    },
                    new Category
                    {
                        Id = 9,
                        Name = "Accessoires audio et vidéo",
                        Active = true,
                    },
                    new Category
                    {
                        Id = 10,
                        Name = "Écrans de projection",
                        Active = true,
                    },
                    new Category
                    {
                        Id = 11,
                        Name = "Télécommandes",
                        Active = true,
                    },
                    new Category
                    {
                        Id = 12,
                        Name = "Cartes mémoire",
                        Active = true,
                    },
                };
            }
        }

        public List<CategoryRequest> SubCategorie
        {
            get
            {
                return new List<CategoryRequest>
                {
                    new CategoryRequest
                    {
                        Name = "Réseaux",
                        Children = new List<CategoryRequest>{
                            new CategoryRequest
                                {
                                    Name = "Routeur Wi-Fi",
                                    Children = new List<CategoryRequest>{
                                        new CategoryRequest
                                        {
                                            Name = "Routeur ADSL"
                                        },
                                        new CategoryRequest
                                        {
                                            Name = "Point d'accès"
                                        },
                                        new CategoryRequest
                                        {
                                            Name = "Routeur Fibre Optique"
                                        },
                                        new CategoryRequest
                                        {
                                            Name = "Routeur 4G/3G"
                                        }
                                    }
                                },
                            new CategoryRequest
                                {
                                    Name = "Switch",
                                    Children = new List<CategoryRequest>{
                                        new CategoryRequest
                                            {
                                                Name = "Switch 10/100"
                                            },
                                        new CategoryRequest
                                            {
                                                Name = "Switch Gigabit"
                                            },
                                        new CategoryRequest
                                            {
                                                Name = "Rakcable"
                                            }
                                    }
                                },
                            new CategoryRequest
                                {
                                    Name = "Clé USB Wi-Fi"
                                },
                            new CategoryRequest
                                {
                                    Name = "Carte Wi-Fi PCI/PCIE"
                                },
                            new CategoryRequest
                                {
                                    Name = "Point D'accès Wi-Fi"
                                },
                            new CategoryRequest
                                {
                                    Name = "Répéteurs Wi-Fi"
                                },
                            new CategoryRequest
                                {
                                    Name = "CPL Powerline"
                                },
                            new CategoryRequest
                                {
                                    Name = "Coffret"
                                },
                            new CategoryRequest
                                {
                                    Name = "Câble Réseau"
                                },
                            new CategoryRequest
                                {
                                    Name = "Connectique"
                                }
                        }
                    },
                    new CategoryRequest
                    {
                        Name = "Gaming",
                        Children = new List<CategoryRequest>{
                            new CategoryRequest
                            {
                                Name = "Gaming",
                                Children = new List<CategoryRequest>{
                                    new CategoryRequest
                                    {
                                        Name = "Souris gamer"
                                    },
                                    new CategoryRequest
                                    {
                                        Name = "Clavier gamer"
                                    },
                                    new CategoryRequest
                                    {
                                        Name = "Casque Gamer"
                                    },
                                    new CategoryRequest
                                    {
                                        Name = "Ventilateur boîtier Gamer"
                                    },
                                    new CategoryRequest
                                    {
                                        Name = "Chaise Gamer"
                                    },
                                    new CategoryRequest
                                    {
                                        Name = "Boîtier PC"
                                    },
                                    new CategoryRequest
                                    {
                                        Name = "Clavier Mécanique"
                                    },
                                    new CategoryRequest
                                    {
                                        Name = "Gaming Combo"
                                    },
                                    new CategoryRequest
                                    {
                                        Name = "Accessoires Gaming"
                                    },
                                    new CategoryRequest
                                    {
                                        Name = "Accessoires PlayStation"
                                    },
                                    new CategoryRequest
                                    {
                                        Name = "Tapis Gamer"
                                    },
                                    new CategoryRequest
                                    {
                                        Name = "Bureau Gaming"
                                    }
                                }
                            }
                        }
                    },
                    new CategoryRequest
                    {
                        Name = "Informatique & Bureau",
                        Children = new List<CategoryRequest>{
                            new CategoryRequest
                            {
                                Name = "Ordinateur Et PC Portable",
                                Children = new List<CategoryRequest>{
                                    new CategoryRequest
                                    {
                                        Name = "Ordinateurs portables"
                                    },
                                    new CategoryRequest
                                    {
                                        Name = "Clavier"
                                    },
                                    new CategoryRequest
                                    {
                                        Name = "Souris"
                                    },
                                    new CategoryRequest
                                    {
                                        Name = "Câble VGA"
                                    },
                                    new CategoryRequest
                                    {
                                        Name = "Chargeur PC Portable"
                                    }
                                }
                            },
                            new CategoryRequest
                            {
                                Name = "Imprimantes",
                                Children = new List<CategoryRequest>{
                                    new CategoryRequest
                                    {
                                        Name = "Imprimantes Thermique"
                                    },
                                    new CategoryRequest
                                    {
                                        Name = "Toner Rainbow"
                                    },
                                }
                            },
                            new CategoryRequest
                            {
                                Name = "Périphériques & Accessoires",
                                Children = new List<CategoryRequest>
                                {
                                    new CategoryRequest
                                    {
                                        Name = "Sac à dos, Sacoche & Housses"
                                    },
                                    new CategoryRequest
                                    {
                                        Name = "Lecteur Carte Mémoire"
                                    },
                                    new CategoryRequest
                                    {
                                        Name = "Ventilateur USB"
                                    },
                                    new CategoryRequest
                                    {
                                        Name = "Boite d'alimentation"
                                    },
                                    new CategoryRequest
                                    {
                                        Name = "Onduleur"
                                    },
                                    new CategoryRequest
                                    {
                                        Name = "Scanner"
                                    }
                                }
                            },
                        }
                    },
                    new CategoryRequest
                    {
                        Name = "Sécurité",
                        Children = new List<CategoryRequest>{
                            new CategoryRequest
                            {
                                Name = "Vidéosurveillance",
                                Children = new List<CategoryRequest>
                                {
                                    new CategoryRequest
                                    {
                                        Name = "Caméras de surveillance"
                                    },
                                    new CategoryRequest
                                    {
                                        Name = "Caméra IP"
                                    },
                                    new CategoryRequest
                                    {
                                        Name = "Enregistreur DVR"
                                    },
                                    new CategoryRequest
                                    {
                                        Name = "Câblage et Connectique"
                                    },
                                    new CategoryRequest
                                    {
                                        Name = "Support Métallique pour Caméra"
                                    },
                                    new CategoryRequest
                                    {
                                        Name = "Surveillance Video Recorder"
                                    }
                                }
                            },
                            new CategoryRequest
                            {
                                Name = "Systéme D'alarme"
                            },
                            new CategoryRequest
                            {
                                Name = "Coffrets Et Alimentation"
                            }
                        }
                    },
                    new CategoryRequest
                    {
                        Name = "Téléphone et Tablettes",
                        Children = new List<CategoryRequest>{
                            new CategoryRequest
                            {
                                Name = "Accessoires Téléphones",
                                Children = new List<CategoryRequest>{
                                    new CategoryRequest
                                    {
                                        Name = "Chargeur"
                                    },
                                    new CategoryRequest
                                    {
                                        Name = "Câble USB"
                                    },
                                    new CategoryRequest
                                    {
                                        Name = "Protecteurs d'écran"
                                    },
                                    new CategoryRequest
                                    {
                                        Name = "Casque VR"
                                    },
                                    new CategoryRequest
                                    {
                                        Name = "Batteries Externes"
                                    },
                                    new CategoryRequest
                                    {
                                        Name = "Accessoires Bluetooth"
                                    },
                                    new CategoryRequest
                                    {
                                        Name = "Accessoire pour selfie"
                                    },
                                    new CategoryRequest
                                    {
                                        Name = "Accessoires Universels"
                                    },
                                    new CategoryRequest
                                    {
                                        Name = "Coques de protection"
                                    },
                                    new CategoryRequest
                                    {
                                        Name = "Support Magnetique"
                                    }
                                }
                            }
                        }                        
                    },
                    new CategoryRequest
                    {
                        Name = "Audio & Vidéo",
                        Children = new List<CategoryRequest>{}                        
                    },
                    new CategoryRequest
                    {
                        Name = "Jeux et Loisirs",
                        Children = new List<CategoryRequest>{}                        
                    },
                    new CategoryRequest
                    {
                        Name = "TV et Satellite",
                        Children = new List<CategoryRequest>{}                        
                    },
                    new CategoryRequest
                    {
                        Name = "Maison et jardin",
                        Children = new List<CategoryRequest>{}                        
                    }
                };
            }
        }

        public ProductRequest[] Products()
        {
            return new ProductRequest[]
            {
                new ProductRequest
                {
                    Name = "DELL Ecran Professional 27 Pouces P2719H FULLHD 1080P",
                    CategoryId = 1,
                    Description = "",
                    Details = "",
                    Images = new string[] { "img-1.png", "img-2.png" },
                    MetaDescription = "",
                    MetaTitle = "",
                    MetaKeywords = "",
                    Active = true,
                    Specification = "",
                    Quantity = 1,
                    NewPrice = 10,
                    OldPrice = 12,
                    IsOffer = true,
                    Offer = 3,
                    Rating = 2,
                }
            };
        }
    }
}
